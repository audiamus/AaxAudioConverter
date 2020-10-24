using System;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {
  class TimeInterval {
    private TimeSpan _begin;
    private TimeSpan _end;
    private TimeSpan _duration;

    public TimeSpan Begin {
      get => _begin;
      set
      {
        _begin = value;
        if (_begin > _end)
          _end = _begin;
        _duration = _end - _begin;
      }
    }
    
    public TimeSpan End {
      get => _end;
      set
      {
        _end = value;
        if (_end < _begin)
          _begin = _end;
        _duration = _end - _begin;
      }
    }

    public TimeSpan Duration {
      get => _duration;
      set {
        _duration = value;
        _begin = TimeSpan.Zero;
        _end = _duration;
      }
    }

    public TimeInterval () { }

    public TimeInterval (TimeSpan duration) {
      _end = duration;
      _duration = duration;
    }

    public TimeInterval (TimeSpan begin, TimeSpan end) {
      _begin = begin;
      _end = end;
      if (_end > _begin)
        _duration = end - begin;
    }

    public TimeInterval (TimeInterval other, TimeSpan offset = default) {
      _begin = other.Begin + offset;
      _end = other.End + offset;
      if (_end > _begin)
        _duration = _end - _begin;
    }

    public TimeInterval Shifted (TimeSpan offset) => new TimeInterval (Begin + offset, End + offset);

    public override string ToString () {
      return $"{Begin.ToStringHMSm()} -> {End.ToStringHMSm()} ({Duration.ToStringHMSm()})";
    }

    public string ToString (TimeSpan offset) =>
      $"[abs: {(Begin+offset).ToStringHMSm()} -> {(End+offset).ToStringHMSm()}]";
    

    public string ToStringEx () => ToString() + $" [{Begin.TotalSeconds,10:000000.000} {End.TotalSeconds,10:000000.000}]";
  }
}
