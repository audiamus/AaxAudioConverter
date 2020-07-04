using System;
using System.Collections.Generic;
using System.Linq;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  partial class Book {

    public class Part {
      public Book Book { get; }

      private bool _namedChaptersNotIn2;

      public AaxFileItem AaxFileItem { get; private set; }
      public int PartNumber { get; private set; }
      public List<Chapter> Chapters { get; set; }
      public List<Chapter> Chapters2 { get; set; }
      public List<Track> Tracks { get; } = new List<Track> ();
      public string ActivationCode { get; set; }
      public bool IsMp3Stream { get; set; }
      public TimeSpan BrandIntro { get; set; }
      public TimeSpan BrandOutro { get; set; }
      public TimeSpan Duration { get; set; }
      public bool ChaptersCurtailed { get; set; }

      public string TmpFileName { get; set; }

      public IReadOnlyList<Chapter> NamedChapters => HasNamedChapters ? (_namedChaptersNotIn2 ? Chapters : Chapters2) : Chapters;

      public bool HasNamedChapters => Chapters?.Count > 0 && Chapters2?.Count > 0;

      public Part (Book book, AaxFileItem fi, int part = 0) {
        Book = book;
        AaxFileItem = fi;
        PartNumber = part;
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
        _namedChaptersNotIn2 = !_namedChaptersNotIn2;
      }

      public void MergeSilences () {
        if (this.Chapters?.Count == 0)
          return;

        var chapter0 = this.Chapters[0];
        TimeSpan startTimeThisChapter = chapter0.Time.Begin;
        if (startTimeThisChapter != TimeSpan.Zero) {
          var shifted = new List<TimeInterval> ();
          foreach (var ivl in chapter0.Silences)
            shifted.Add (ivl.Shifted (startTimeThisChapter));
          chapter0.Silences = shifted;
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
              if (ivl.Begin <= chapter0.Silences.Last ().End) {
                ivl = new TimeInterval (chapter0.Silences.Last ().Begin, ivl.End);
                chapter0.Silences.RemoveAt (chapter0.Silences.Count - 1);
              }
            }

            chapter0.Silences.Add (ivl);
          }
        }

        chapter0.Time.End = this.Chapters.Last ().Time.End;
        chapter0.TmpFileName = TmpFileName;
        chapter0.Name = null;
        this.Chapters.Clear ();
        this.Chapters.Add (chapter0);
      }

      public void CreateCueSheet (IConvSettings settings) {
        Log (3, this, () => this.ToString());
        // do not trust the settings range check. Enforce track length between 3 and 15 min. 
        int trackDurMins = Math.Max (3, Math.Min (15, (int)settings.TrkDurMins));

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
        int affected = this.Chapters.Where (c => !c.TimeAdjustment.IsNull ()).Count ();
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
