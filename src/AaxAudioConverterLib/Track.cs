using System;
using System.Collections.Generic;

namespace audiamus.aaxconv.lib {


  enum ETrackState { initial, current, complete, aborted }

  class Track {
    public TimeInterval Time { get; private set; }
    public string FileName { get; set; }
    public string Title { get; set; }
    public ETrackState State { get; set; }
    public Chapter Chapter { get; set; }
    public List<Chapter> MetaChapters { get; set; }

    public Track (TimeSpan duration) {
      Time = new TimeInterval (duration);
    }

    public Track (TimeSpan begin, TimeSpan end) {
      Time = new TimeInterval (begin, end);
    }

    public override string ToString () {
      string chapter = string.Empty;
      if (Chapter != null)
        chapter = $", Ch: {Chapter}";
      return $"{Time}{chapter}";
    }
  }

}

