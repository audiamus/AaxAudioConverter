#define xEXTRA

using System;
#if EXTRA
using static audiamus.aux.Logging;
#endif

namespace audiamus.aaxconv.lib {

  class ThreadProgress : IDisposable {
#if EXTRA
    const int LVL = 3; 

    private static int __id = 0;
    private readonly int _id = ++__id;

    private string ID => $"{{#{_id}}} ";
#endif

    const int _1000 = 1000;
    readonly Action<ProgressMessage> _report;

    private int _valuePerMille;

    public ThreadProgress (Action<ProgressMessage> report) {
#if EXTRA
      Log (LVL, this, () => ID);
#endif
      _report = report;
    }

    public void Dispose () {
#if EXTRA
      Log (LVL, this, () => ID);
#endif
      int inc = _1000 - _valuePerMille;
      if (inc > 0) {
#if EXTRA
        Log (LVL, this, () => $"{ID}{inc}");
#endif
        _report?.Invoke (new ProgressMessage {
          IncStepsPerMille = (uint)inc
        });
      }
    }

    public void Report (double value) {
#if EXTRA
      Log (LVL, this, () => $"{ID}val={value}");
#endif
      int val = (int)(value * _1000);
      int total = Math.Min (_1000, val);
      int inc = total - _valuePerMille;
      _valuePerMille = total;
      if (inc > 0) {
#if EXTRA
        Log (LVL, this, () => $"{ID}{inc}");
#endif
        _report?.Invoke (new ProgressMessage {
          IncStepsPerMille = (uint)inc
        });
      }
    }


  }

}
