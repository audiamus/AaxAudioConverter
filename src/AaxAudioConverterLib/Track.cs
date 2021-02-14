using System;
using System.Collections.Generic;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {


  enum ETrackState { initial, current, complete, aborted }

  interface ITrack {
    IChapter Chapter { get; }
    ITimeInterval Time { get;  }
    string FileName { get;  }
    string Title { get; }
    bool HasMinDuration { get; }
  }

  class Track : ITrack {
    static readonly TimeSpan MIN_DURATION = TimeSpan.FromMilliseconds (Chapter.MS_MIN_CHAPTER_LENGTH);

    public TimeInterval Time { get; private set; }
    public string FileName { get; set; }
    public string Title { get; set; }
    public ETrackState State { get; set; }
    public Chapter Chapter { get; set; }
    public List<Chapter> MetaChapters { get; set; }

    public bool HasMinDuration => Time.Duration >= MIN_DURATION;

    IChapter ITrack.Chapter => Chapter;
    ITimeInterval ITrack.Time => Time;

    public Track (TimeSpan duration) {
      Time = new TimeInterval (duration);
    }

    public Track (TimeSpan begin, TimeSpan end) {
      Time = new TimeInterval (begin, end);
    }

    public static bool MatchesMinDuration (TimeInterval time) {
      if (time is null)
        return true;
      return time.Duration >= MIN_DURATION;
    }

    public override string ToString () {
      string chapter = string.Empty;
      if (!Chapter.IsNull ())
        chapter = $", Ch: {Chapter}";
      return $"{Time}{chapter}";
    }
  }

}

