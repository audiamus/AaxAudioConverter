using System;
using System.Collections.Generic;
using System.Linq;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {

  interface IChapter {
    ITimeInterval Time { get; }
    string Name { get; }
  }

  class Chapter : IChapter {

    public const int MS_MIN_CHAPTER_LENGTH = 1;
    public static readonly TimeSpan TS_MIN_LAST_CHAPTER_LENGTH = TimeSpan.FromSeconds (1);

    static readonly TimeSpan TS_MIN_CHAPTER_LENGTH = TimeSpan.FromMilliseconds (MS_MIN_CHAPTER_LENGTH);
    static readonly TimeSpan TS_EPS_SILENCE = TimeSpan.FromMilliseconds (10);
    static readonly TimeSpan TS_MAX_LAST_SILENCE = TimeSpan.FromSeconds (2);
    static readonly TimeSpan TS_CATCH_RANGE = TimeSpan.FromSeconds (30);
    static readonly TimeSpan TS_PART_TITLE_LENGTH = TimeSpan.FromSeconds (10);
    static readonly TimeSpan TS_LONGER_SILENCE = TimeSpan.FromMilliseconds (900);
    static readonly TimeSpan TS_SHORT_SILENCE = TimeSpan.FromMilliseconds (500);
    static readonly TimeSpan TS_SILENCE_ANTE = TimeSpan.FromMilliseconds (300);

    internal class TimeAdjustments {
      public TimeSpan Begin;
      public TimeSpan End;

      public override string ToString () => $"adj beg={Begin.ToStringHMSm ()}, end={End.ToStringHMSm ()}";

      public string ToString (TimeInterval offset) =>
        $"[abs: {(Begin + offset.Begin).ToStringHMSm ()} -> {(End + offset.End).ToStringHMSm ()}]";
    }

    public TimeInterval Time { get; } = new TimeInterval ();
    public string Name { get; set; }
    public List<TimeInterval> Silences { get; set; }

    public bool IsPaddingChapter { get; set; }

    public string TmpFileName { get; set; }

    public TimeAdjustments TimeAdjustment { get; private set; }

    ITimeInterval IChapter.Time => Time;

    public Chapter () { }

    public Chapter (TimeSpan duration) {
      Time.Duration = duration;
    }

    public Chapter (TimeSpan begin, TimeSpan end) {
      Time.Begin = begin;
      Time.End = end;
    }

    public Chapter (Chapter other, bool withSilences = false) : this (other.Time.Begin, other.Time.End) {
      Name = other.Name;
      if (withSilences)
        Silences = other.Silences.ToList ();
    }

    public void CreateCueSheet (Book.Part part, int trackDurMins, bool splitTimeMode) {
      Log (3, this, () => this.ToString ());
      
      // chapter duration in sec
      double chLenSec = this.Time.Duration.TotalSeconds;

      // rounded number of tracks in chapter, observing desired track duration 
      int numTracks = Math.Max ((int)(chLenSec / (trackDurMins * 60) + 0.5), 1);

      // average track length in sec
      double aveTrackLen = chLenSec / numTracks;

      // average track length as TimeSpan
      var tsAveTrackLen = TimeSpan.FromSeconds (aveTrackLen);

      // search range for silence at desired end of track is +/- 1/3 ave track length
      var tsSearchRange = TimeSpan.FromSeconds (aveTrackLen / 3);

      // start 1st track at zero 
      // unless in time split mode where we start at the actual beginning of the chapter
      var tsStart = TimeSpan.Zero;
      if (splitTimeMode)
        tsStart = this.Time.Begin;

      // desired end of 1st track
      var tsEnd = tsStart + tsAveTrackLen;

      // max end is chapter duration unless in time split mode
      var tsChEnd = this.Time.Duration - TS_EPS_SILENCE;
      if (splitTimeMode)
        tsChEnd = this.Time.End;

      // filter silences
      var silences = this.Silences.Where (s => s.Duration >= TS_SHORT_SILENCE).ToList ();

      // while chapter length has not been reached, will be checked in loop
      while (true) {

        if (tsEnd < tsChEnd) {
          // upper search limit for silence
          var tsUpper = tsEnd + tsSearchRange;
          // lower search limit for silence
          var tsLower = tsEnd - tsSearchRange;

          // take the easy road using LINQ, find nearest silence or none, above and below
          var silUp = silences.Where (t => t.Begin >= tsEnd && t.Begin < tsUpper).FirstOrDefault ();
          var silDn = silences.Where (t => t.End > tsLower && t.End <= tsEnd).LastOrDefault ();

          // which silence shall it be
          TimeInterval sil = null;
          if (!(silUp is null || silDn is null)) {
            // up and down found, use nearest
            var deltaUp = silUp.Begin - tsEnd;
            var deltaDn = tsEnd - silDn.End;
            if (deltaUp < deltaDn)
              sil = silUp;
            else
              sil = silDn;
          } else {
            // not both found, use any present
            if (!(silUp is null))
              sil = silUp;
            else if (!(silDn is null))
              sil = silDn;
          }

          // silence has been found
          if (!(sil is null)) {
            // actual track end
            // add half of the silence interval, put other half of the silence into the next track. 
            // However, cut in stream will not be precise.
            var tsActualEnd = sil.Begin + TimeSpan.FromTicks (sil.Duration.Ticks / 2);

            // check for overshooting
            if (tsActualEnd > tsChEnd)
              tsActualEnd = tsChEnd;

            // actual difference
            var tsDelta = tsActualEnd - tsEnd;

            // create new track item
            var track = new Track (tsStart, tsActualEnd) {
              Chapter = this
            };
            part.Tracks.Add (track);

            // set for next track
            tsStart = tsActualEnd;
            tsEnd = tsStart + tsAveTrackLen - tsDelta;
            if (tsEnd + tsSearchRange > tsChEnd)
              tsEnd = tsChEnd;

          } else {
            // silence not found, extend track
            tsEnd += tsAveTrackLen;
          }
        }

        // at the end of the chapter
        if (tsEnd >= tsChEnd) {
          // last track without silence search
          var track = new Track (tsStart, tsChEnd) {
            Chapter = this
          };
          part.Tracks.Add (track);

          // done
          break;
        }
      }
    }


    public void DetermineTimeAdjustment (Chapter nextChapter) {
      if (Silences.IsNullOrEmpty ())
        return;
      Log (3, this, () => $"{this}");

      if (handlePartTitle (nextChapter))
        return;
      
      handleChapterMark (nextChapter);
    }

    private void setNewBgn (TimeSpan begin) {
      Log (3, this, () => $"named: {this}, bgn time replaced with: {begin.ToStringHMSm()}");
      if (TimeAdjustment is null)
        TimeAdjustment = new TimeAdjustments ();
      TimeAdjustment.Begin = begin - Time.Begin; 
    }

    public IEnumerable<Chapter> SetNewEnd (TimeSpan end, Chapter nextChapter, bool always) {
      if (!always) {
        bool isSil = true;
        if (end < Time.End) {
          if (Silences != null) {
            TimeSpan relEnd = end - Time.Begin;
            isSil = Silences.Where (s => s.Begin <= relEnd && s.End >= relEnd).Any ();
          }
        } else if (nextChapter?.Silences != null) {
          TimeSpan relBgn = end - nextChapter.Time.Begin;
          isSil = nextChapter.Silences.Where (s => s.Begin <= relBgn && s.End >= relBgn).Any ();
        };
        if (!isSil)
          return null;
      }

      Log (3, this, () => $"named: {this}, end time replaced with: {end.ToStringHMSm()}");
      var chapters = new List<Chapter> ();
      if (TimeAdjustment is null)
        TimeAdjustment = new TimeAdjustments ();
      TimeAdjustment.End = end - Time.End;
      chapters.Add (this);
      if (!nextChapter.IsNull ()) {
        nextChapter.setNewBgn (end);
        chapters.Add (nextChapter);
      }
      return chapters;
    }

    public void AdjustTime (Chapter nextChapter) {
      if (TimeAdjustment is null)
        return;

      Log (3, this, () => $"{this}; {TimeAdjustment}");

      // if the chapter becomes longer, there may be silence at the beginning of the next chapter that must go here.
      // if the chapter becomes shorter, there may be silence at the end of this chapter that must go to the next chapter.
      // it will also affect partial silence between both chapters which has to be split
      // solved by simply duplicating affected silences

      if (nextChapter?.Silences != null && TimeAdjustment.End != TimeSpan.Zero) {
        TimeSpan adjEnd = Time.Duration + TimeAdjustment.End;
        if (TimeAdjustment.End > TimeSpan.Zero) {
          // need all silences in next with a begin earlier than adjustment
          var affected = nextChapter.Silences.Where (s => s.Begin <= TimeAdjustment.End).ToList();

          foreach (var aff in affected) {
            var sil = new TimeInterval (aff, Time.Duration);
            Silences.Add (sil);
          }


        } else { // < Zero
          // need all silences in this with an end later than adjustment
          var affected = Silences.Where (s => s.End >= adjEnd).ToList();

          foreach (var aff in affected) {
            var sil = new TimeInterval (aff, -Time.Duration);
            nextChapter.Silences.Add (sil);
          }
          nextChapter.Silences.Sort ((x, y) => TimeSpan.Compare(x.Begin, y.Begin));

        }
      }


      Time.Begin += TimeAdjustment.Begin;
      Time.End += TimeAdjustment.End;

      if (!Silences.IsNullOrEmpty()) {
        var zeros = new List<int> ();
        if (TimeAdjustment.Begin != TimeSpan.Zero) {
          for (int i = 0; i < Silences.Count; i++) {
            var sil = Silences[i];
            if (TimeAdjustment.Begin > TimeSpan.Zero) {
              sil.Begin -= TimeAdjustment.Begin;
              sil.End -= TimeAdjustment.Begin;
            } else {
              sil.End -= TimeAdjustment.Begin;
              sil.Begin -= TimeAdjustment.Begin;
            }

            if (sil.Begin < TimeSpan.Zero)
              sil.Begin = TimeSpan.Zero;
            if (sil.Duration == TimeSpan.Zero)
              zeros.Add (i);
          }

          zeros.Reverse ();
          foreach (int i in zeros)
            Silences.RemoveAt (i);

        }
        if (TimeAdjustment.End != TimeSpan.Zero) {
          while (Silences.Last().Begin > Time.Duration )
            Silences.RemoveAt (Silences.Count - 1);

          var sil = Silences.Last ();
          if (sil.End > Time.Duration)
            sil.End = Time.Duration;

          //var sil = Silences.Last ();
          //if (sil.Begin > Time.Duration)
          //  Silences.Remove (sil);
          //else 
          //  sil.End = Time.Duration; // should equal chapter duration
        }
      }

      TimeAdjustment = null;
      Log (3, this, () => $"{this}, adjusted");
    }

    private bool startsWithSilence () {
      if (Silences.IsNullOrEmpty ())
        return false;
      bool atbegin = Silences[0].Begin < TS_EPS_SILENCE;
      return atbegin;
    }

    private bool endsWithSilence (TimeInterval silence = null) {
      if (silence is null) {
        if (Silences.IsNullOrEmpty ())
          return false;
        silence = Silences.Last ();
      }
      bool atend = silence.End >= (Time.Duration - TS_EPS_SILENCE);
      return atend;
    }

    private void forwardAdjustment (TimeSpan adjust) {
      if (TimeAdjustment is null)
        TimeAdjustment = new TimeAdjustments ();
      TimeAdjustment.Begin -= adjust;
      Log (3, this, () => $"{this}; {TimeAdjustment}, {TimeAdjustment.ToString(Time)}");
    }

    private void handleChapterMark (Chapter nextChapter) {
      TimeInterval lastSilence = Silences.Last ();

      // last silence extends into next chapter?
      if (this.endsWithSilence (lastSilence)) {
        bool nextChapterStartsWithSilence = nextChapter?.startsWithSilence () ?? false;

        // is this a long silence?
        if (lastSilence.Duration > TS_MAX_LAST_SILENCE) {
          // but next chapter already starts with silence?
          if (nextChapterStartsWithSilence)
            return;
        } else {
          // If we end with a silence but the next chapter starts without one, then let's split our silence in half
          if (!nextChapterStartsWithSilence)
            setAdjustment (nextChapter, lastSilence);

          return;
        }
      }

      // the next chapter may start with silence but it might not be the right one. 
      if (nextChapter?.startsWithSilence () ?? false) {
        var sil = Silences.Where (s => s.End > this.Time.Duration - TS_PART_TITLE_LENGTH && s.Duration > TS_LONGER_SILENCE).LastOrDefault ();
        if (sil.IsNull())
          return;
      }

      // this chapter does not end and next does not begin with silence 
      // so we look closer

      // only treat it, if close to the end
      if (lastSilence.End < Time.Duration - TS_CATCH_RANGE) {
        Log (3, this, () => $"{this}; out of range: {lastSilence}");
        return;
      }

      if (!nextChapter.IsNull ()) {
        // alternatively merge nearest longer silences from next and this into tmp
        var silences = new List<TimeInterval> ();
        silences.AddRange (Silences.Where (s => s.End > this.Time.Duration - TS_PART_TITLE_LENGTH && s.Duration > TS_LONGER_SILENCE).Select (s => new TimeInterval (s)));
        silences.AddRange (nextChapter.Silences.Where (s => s.Begin < TS_PART_TITLE_LENGTH && s.Duration > TS_LONGER_SILENCE).Select (s => new TimeInterval (s, this.Time.Duration)));

        // pick the one closest to the chapter mark
        var silence = closestToChapterMark (silences, true);
        if (!silence.IsNull ()) {
          lastSilence = silence;
          Log (3, this, () => $"{this}; assumed end #{silences.IndexOf (silence) + 1}/{silences.Count}: {silence}, [abs -> {(Time.Begin + silence.End).ToStringHMSm()}]");
        }
      }

      setAdjustment (nextChapter, lastSilence);
    }

    private bool handlePartTitle (Chapter next) {
      // ignore PreChapter
      if (IsPaddingChapter)
        return false;

      // chapter must be very short to be analysed here 
      // and must have a longer successor
      if (this.Time.Duration > TS_PART_TITLE_LENGTH ||
          next.IsNull () ||
          next.Time.Duration < TS_PART_TITLE_LENGTH ||
          next.Silences.IsNullOrEmpty ())
        return false;

      // pattern:       "-- Part I [- Title] -- Chapter 1 [- Title] -- Text" 
      // chapter mark:        >               <


      bool spanning = this.endsWithSilence () && next.startsWithSilence ();

      // merge the silences from next and this into tmp
      var silences = new List<TimeInterval> ();
      silences.AddRange (Silences.Select (s => new TimeInterval (s)));
      silences.AddRange (next.Silences.Where (s => s.Begin < TS_PART_TITLE_LENGTH).Select (s => new TimeInterval (s, this.Time.Duration)));

      if (spanning) {
        int idx = Silences.Count;
        silences[idx - 1].End += silences[idx].Duration;
        silences.RemoveAt (idx);
      }

      if (startsWithSilence())
        silences.RemoveAt (0);

      silences = silences.Where (s => s.Duration > TS_LONGER_SILENCE).ToList ();

      TimeInterval silence = closestToChapterMark (silences);

      if (silence is null) {
        Log (3, this, () => $"{this}; silence is <null>");
        return true;
      }
      Log (3, this, () => $"{this}; assumed end #{silences.IndexOf (silence) + 1}/{silences.Count}: {silence}");

      setAdjustment (next, silence);
      return true;
    }

    private TimeInterval closestToChapterMark (List<TimeInterval> silences, bool preferThis = false) {
      // pick the one closest to the chapter mark
      var distThis = TS_PART_TITLE_LENGTH;
      var distNext = TS_PART_TITLE_LENGTH;
      TimeInterval silThis = null;
      TimeInterval silNext = null;
      foreach (var sil in silences) {
        if (sil.End < this.Time.Duration) {
          var dist = this.Time.Duration - sil.End;
          if (dist > distThis)
            continue;
          distThis = dist;
          silThis = sil;
        } else {
          var dist = sil.Begin - this.Time.Duration;
          if (dist > distNext)
            continue;
          distNext = dist;
          silNext = sil;
        }
      }

      if (silThis.IsNull ())
        return silNext;
      if (silNext.IsNull ())
        return silThis;

      if (preferThis) {
        if (silThis.Duration > silNext.Duration)
          return silThis;
        else
          return silNext;
      } else {
        if (distThis < distNext)
          return silThis;
        else
          return silNext;
      }

    }

    private void setAdjustment (Chapter nextChapter, TimeInterval silence) {
      if (silence is null)
        return;

      if (TimeAdjustment is null)
        TimeAdjustment = new TimeAdjustments ();

      TimeSpan newSilDuration = silence.Duration - TS_SILENCE_ANTE;
      if (newSilDuration < TimeSpan.Zero)
        newSilDuration = new TimeSpan (silence.Duration.Ticks / 3);

      TimeSpan newEnd = silence.Begin + newSilDuration;
      TimeSpan adjust = Time.Duration - newEnd;
      TimeAdjustment.End -= adjust;

      Log (3, this, () => $"{this}; {TimeAdjustment}, {TimeAdjustment.ToString(Time)}");
      nextChapter?.forwardAdjustment (adjust);
    }


    public override string ToString () => $"{Time}: {(IsPaddingChapter ? "<Padding>" : $"\"{Name}\"")}";

    public string ToStringEx () => $"{Time.ToStringEx()}: \"{Name}\"";
  }

}

