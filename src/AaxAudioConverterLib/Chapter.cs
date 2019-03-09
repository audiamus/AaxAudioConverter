using System;
using System.Collections.Generic;

namespace audiamus.aaxconv.lib {

  class Chapter {
    public readonly TimeInterval Time = new TimeInterval ();
    public string Name;
    public List<TimeInterval> Silences { get; set; }

    public string TmpFileName { get; set; }

    public Chapter () { }
    public Chapter (TimeSpan duration) {
      Time.Duration = duration;
    }

    public override string ToString () {
      return $"{Time}: {Name}";
    }
  }

}

