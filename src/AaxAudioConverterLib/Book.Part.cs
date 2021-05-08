using System;
using System.Collections.Generic;
using System.Linq;
using audiamus.aux.ex;
using audiamus.aaxconv.lib.ex;
using static audiamus.aux.Logging;
using audiamus.aux;

namespace audiamus.aaxconv.lib {
  partial class Book {

    public class Part {
      public Book Book { get; }

      private bool _namedChaptersIn1;
      private bool? _isMp3Stream;

      public AaxFileItem AaxFileItem { get; private set; }
      public int PartNumber { get; private set; }
      public List<Chapter> Chapters { get; set; }
      public List<Chapter> Chapters2 { get; set; }
      public List<Track> Tracks { get; } = new List<Track> ();
      public string ActivationCode { get; set; }
      public TimeSpan BrandIntro { get; set; }
      public TimeSpan BrandOutro { get; set; }
      public TimeSpan Duration { get; set; }
      public bool ChaptersCurtailed { get; set; }
      public string SKU { get; set; }

      public string TmpFileName { get; set; }
      public string TmpFileNameIntermedCopy { get; set; }
      public string TmpFileNameAACFixed { get; set; }
      public string ApplicableInFileNameForTranscode => TmpFileName ?? TmpFileNameAACFixed ?? AaxFileItem.FileName;

      public bool FFmpegPrefer64Bit { get; private set; }

      public bool IsMp3Stream {
        get
        {
          if (_isMp3Stream.HasValue)
            return _isMp3Stream.Value;
          else
            return AaxFileItem.IsAA;          
        }
        set => _isMp3Stream = value;
      }

      public IReadOnlyList<Chapter> NamedChapters => HasNamedChapters ? (_namedChaptersIn1 ? Chapters : Chapters2) : Chapters;
      private IReadOnlyList<Chapter> EmbeddedChapters => HasNamedChapters ? (_namedChaptersIn1 ? Chapters2 : Chapters) : Chapters;

      public bool HasNamedChapters => Chapters?.Count > 0 && Chapters2?.Count > 0;

      public Part (Book book, AaxFileItem fi, int part = 0) {
        Book = book;
        AaxFileItem = fi;
        PartNumber = part;
        Duration = fi.Duration;
      }

      public void ComplementChapterNames () {
        if (Chapters is null)
          return;
        for (int i = 0; i < Chapters.Count; i++) {
          var ch = Chapters[i];
          if (ch.Name.IsNullOrWhiteSpace ())
            ch.Name = $"({i + 1})";
        }
      }

      public void SwapChapters () {
        var tmp = Chapters;
        Chapters = Chapters2;
        Chapters2 = tmp;
        _namedChaptersIn1 = !_namedChaptersIn1;
      }

      public IEnumerable<Chapter> AdjustTimesWithPreference (EPreferEmbeddedChapterTimes preference) {
        Log (3, this, () => $"\"{Book.SortingTitle.Shorten ()}\", preference={preference}");

        // from adjustment by silence
        adjustTimes ();

        if (HasNamedChapters && preference != EPreferEmbeddedChapterTimes.no)
          return PreferEmbeddedChapterTimes (preference == EPreferEmbeddedChapterTimes.always);
        else
          return null;
      }

      public bool IsTrancodeConversion (IConvSettings settings) {
        bool isMp3 = IsMp3Stream;
        bool wantMp3 = settings.ConvFormat == EConvFormat.mp3;
        bool isTranscode = isMp3 != wantMp3;

        var (abr, _) = settings.ApplicableBitRate (AaxFileItem.AvgBitRate);
        bool changeBitRate = abr != 0;

        isTranscode |= changeBitRate;
        return isTranscode;
      }

      static readonly TimeSpan DurationFFmpeg64Bit = TimeSpan.FromHours (32);

      // long books, single mode: default 32bit FFmpeg may run out of memory
      public void SetFFmpegPrefer64Bit (IConvSettings settings) {
        if (settings.ConvMode != EConvMode.single) 
          return;

        if (!settings.FFMpegDirectory.IsNullOrWhiteSpace ())
          return;

        TimeSpan durationFFmpeg64Bit = DurationFFmpeg64Bit;
        if (settings.FFmpeg64bitHours > 0)
          durationFFmpeg64Bit = TimeSpan.FromHours (settings.FFmpeg64bitHours);
        if (Duration < durationFFmpeg64Bit)
          return;

        FFmpegPrefer64Bit = ApplEnv.Is64BitOperatingSystem;
        Log (3, this, () => 
          $"\"{Book.SortingTitle.Shorten ()}\" {nameof(FFmpegPrefer64Bit)}={FFmpegPrefer64Bit}");
      }


      private void adjustTimes () {

        Log (3, this, () => $"\"{Book.SortingTitle.Shorten ()}\"");

        //string infile = ApplicableInFileNameForTranscode;
        for (int i = 0; i < Chapters.Count; i++) {
          Chapter chapter = Chapters[i];
          Chapter nextChapter = null;
          if (i < Chapters.Count - 1)
            nextChapter = Chapters[i + 1];
          chapter.AdjustTime (nextChapter);
        }
      }


      internal IReadOnlyList<Chapter> PreferEmbeddedChapterTimes (bool always) {
        if (!HasNamedChapters)
          return null;

        Log (3, this, () => $"\"{Book.SortingTitle.Shorten ()}\", always={always}");

        var matchingChaptersNamedToEmbedded = new Dictionary<Chapter, Chapter> ();
        findMatchingChapters (matchingChaptersNamedToEmbedded);

        var chapters = new List<Chapter> ();
        foreach (var kvp in matchingChaptersNamedToEmbedded) {
          Chapter nmCh = kvp.Key;
          int idx = NamedChapters
            .Select ((elem, index) => new { elem, index })
            .First (p => p.elem == nmCh)
            .index;
          Chapter nmChNxt = null;
          if (idx >= 0 && idx < NamedChapters.Count - 1)
            nmChNxt = NamedChapters[idx + 1];

          Chapter ebCh = kvp.Value;

          Log (3, this, () => $"named: {nmCh}, using embedded: {ebCh}");

          // HACK test only
          //TimeSpan end = ebCh.Time.End + TimeSpan.FromSeconds (1);
          //nmCh.SetNewEnd (end, nmChNxt, always);

          var result = nmCh.SetNewEnd (ebCh.Time.End, nmChNxt, always);
          if (!result.IsNullOrEmpty ())
            chapters.AddRange (result);
        }

        adjustTimes ();

        chapters = chapters.Distinct ().ToList ();
        return chapters;
      }

      private void findMatchingChapters (Dictionary<Chapter, Chapter> matchingChaptersNamedToEmbedded) {
        var embeddedExcludes = new HashSet<Chapter> ();

        var namedChapters = NamedChapters.ToList ();
        namedChapters.Remove (namedChapters.Last ());
        while (true) {
          var matchingChaptersEmbeddedToNamed = new Dictionary<Chapter, List<Chapter>> ();

          // for each named chapter end time find closest embedded chapter end time
          foreach (Chapter nmCh in namedChapters) {
            double minDelta = double.MaxValue;
            Chapter ebMatchCh = null;
            for (int j = 0; j < EmbeddedChapters.Count - 1; j++) {
              Chapter ebCh = EmbeddedChapters[j];
              if (embeddedExcludes.Contains (ebCh))
                continue;
              double delta = Math.Abs ((nmCh.Time.End - ebCh.Time.End).TotalSeconds);
              if (delta < minDelta) {
                minDelta = delta;
                ebMatchCh = ebCh;
              }
            }
            // if nearer than threshold then replace times
            const double THRSH = 10; // sec
            if (minDelta > THRSH) {
              matchingChaptersNamedToEmbedded.Remove (nmCh);
              continue;
            }

            matchingChaptersNamedToEmbedded[nmCh] = ebMatchCh;
            bool found = matchingChaptersEmbeddedToNamed.TryGetValue (ebMatchCh, out var list);
            if (!found) {
              list = new List<Chapter> ();
              matchingChaptersEmbeddedToNamed[ebMatchCh] = list;
            }
            list.Add (nmCh);
          }

          namedChapters.Clear ();
          var multipleReplacements = matchingChaptersEmbeddedToNamed.Where (kvp => kvp.Value.Count > 1).ToList ();

          if (!multipleReplacements.Any ())
            break;

          foreach (var kvp in multipleReplacements) {
            // key is embedded ch
            Chapter ebCh = kvp.Key;

            double minDurDelta = double.MaxValue;
            Chapter nmMatchCh = null;

            // val are named ch
            foreach (Chapter nmCh in kvp.Value) {
              double delta = Math.Abs ((nmCh.Time.Duration - ebCh.Time.Duration).TotalSeconds);
              if (delta < minDurDelta) {
                minDurDelta = delta;
                nmMatchCh = nmCh;
              }
            }

            embeddedExcludes.Add (ebCh);
            namedChapters.AddRange (kvp.Value.Where (ch => ch != nmMatchCh));
          }

        }
      }

      public string TrackId (Track track) =>
        $"{PartNumber}.{Tracks?.IndexOf(track)+1}";
      public string ChapterId (Chapter chapter) =>
        $"{PartNumber}.{Chapters?.IndexOf(chapter)+1}";
  

      public void SetMetaChapters (IConvSettings settings) {
        switch (settings.ConvMode) {
          case EConvMode.chapters:
          case EConvMode.splitChapters:
            setMetaChaptersSingle ();
            break;
          case EConvMode.splitTime:
            setMetaChaptersMulti ();
            break;
        }
      }

      private void setMetaChaptersSingle () {
        if (Tracks.IsNullOrEmpty ())
          return;
        Log (3, this, () => this.ToString ());

        foreach (var track in Tracks) {
          Chapter ch = new Chapter (track.Time.Duration) {
            Name = track.Chapter.Name
          };
          track.MetaChapters = new List<Chapter> {
            ch
          };
        }
      }

      private void setMetaChaptersMulti () {
        if (Chapters2.IsNullOrEmpty () || Tracks.IsNullOrEmpty())
          return;

        Log (3, this, () => this.ToString ());

        foreach (var track in Tracks) {
          var chapters = Chapters2
            .Where (ch => 
              (ch.Time.End >= track.Time.Begin && ch.Time.Begin <= track.Time.End) 
              || ch.Time.Begin <= track.Time.Begin && ch.Time.End >= track.Time.End)
            .Select (ch => new Chapter(ch))
            .ToList();
          if (chapters.Count == 0)
            continue;

          foreach (var chapter in chapters) {
            chapter.Time.Begin -= track.Time.Begin;
            chapter.Time.End -= track.Time.Begin;
          }

          chapters.First ().Time.Begin = TimeSpan.Zero;
          chapters.Last ().Time.End = track.Time.Duration;

          track.MetaChapters = chapters;

          Log (3, this, () => $"{track}, #metachapters={track.MetaChapters.Count}");
        }
      }

      public int InsertPaddingChapters () {
        int chCount = Chapters.Count;

        if (chCount == 0)
          return 0;

        var ch = Chapters.First();
        if (ch.Time.Begin > TimeSpan.Zero && !ch.IsPaddingChapter) {
          var preChapter = new Chapter (ch.Time.Begin) { IsPaddingChapter = true };
          Chapters.Insert (0, preChapter);
        }

        ch = Chapters.Last();
        if (ch.Time.End < this.Duration && !ch.IsPaddingChapter) {
          var postChapter = new Chapter (ch.Time.End, this.Duration) { IsPaddingChapter = true };
          Chapters.Add (postChapter);
        }

        return Chapters.Count - chCount;
      }

      public void RemovePaddingChapters () {
        if (Chapters.FirstOrDefault()?.IsPaddingChapter ?? false)
          Chapters.RemoveAt (0);
        if (Chapters.LastOrDefault()?.IsPaddingChapter ?? false)
          Chapters.RemoveAt (Chapters.Count - 1);
       }

      public void MergeSilences () {
        if (this.Chapters.IsNullOrEmpty())
          return;

        Log (3, this, () => this.ToString ());

        var aggrChapter = new Chapter (Chapters[0], true);
        TimeSpan startTimeThisChapter = aggrChapter.Time.Begin;
        if (startTimeThisChapter != TimeSpan.Zero) {
          var shifted = new List<TimeInterval> ();
          foreach (var ivl in aggrChapter.Silences)
            shifted.Add (ivl.Shifted (startTimeThisChapter));
          aggrChapter.Silences = shifted;
        }

        for (int i = 1; i < this.Chapters.Count; i++) {
          var chapter = this.Chapters[i];
          if (chapter.Silences is null)
            continue;

          startTimeThisChapter = chapter.Time.Begin;

          for (int j = 0; j < chapter.Silences.Count; j++) {
            var ivl = chapter.Silences[j];

            ivl = ivl.Shifted (startTimeThisChapter);

            if (j == 0) {
              if (ivl.Begin <= aggrChapter.Silences.Last ().End) {
                ivl = new TimeInterval (aggrChapter.Silences.Last ().Begin, ivl.End);
                aggrChapter.Silences.RemoveAt (aggrChapter.Silences.Count - 1);
              }
            }

            aggrChapter.Silences.Add (ivl);
          }
        }

        aggrChapter.Time.End = this.Chapters.Last ().Time.End;
        aggrChapter.TmpFileName = TmpFileName;
        aggrChapter.Name = null;
        SwapChapters ();
        this.Chapters = new List<Chapter> {
          aggrChapter
        };
      }

      public void CreateCueSheet (IConvSettings settings) {
        Log (3, this, () => this.ToString());
        // Do not trust the settings range check. Enforce track length between 3 and 15/90 min. 

        var (_, _, trackDurMins) = settings.TrkDurMins.TrackDuration (settings.ConvMode);

        // expects all chapters in individual files, each starting at time 0

        // tracks per chapter
        foreach (var chapter in Chapters) {
          chapter.CreateCueSheet (this, trackDurMins, settings.ConvMode == EConvMode.splitTime);
        }
      }

      public int DetermineTimeAdjustments (Book book) {
        Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\", part {this.PartNumber}");
        for (int i = 0; i < this.Chapters.Count; i++) {
          Chapter chapter = this.Chapters[i];
          Chapter chapterNext = null;
          if (i < this.Chapters.Count - 1)
            chapterNext = this.Chapters[i + 1];
          else if (!this.ChaptersCurtailed)
            break;
          chapter.DetermineTimeAdjustment (chapterNext);
        }
        int affected = this.Chapters.Where (c => !c.TimeAdjustment.IsNull () && !c.IsPaddingChapter).Count ();
        Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\", part {this.PartNumber}, #affected={affected}");
        return affected;
      }

      public override string ToString () {
        string part = string.Empty;
        if (PartNumber != 0)
          part = $" Part {PartNumber}";
        return $"\"{Book.SortingTitle.Shorten()}\"{part}, {AaxFileItem.Duration.ToStringHMSm ()}, #Ch={Chapters?.Count}, #Tr={Tracks?.Count}";
      }

    }

  }
}
