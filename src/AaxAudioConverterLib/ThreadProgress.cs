using System;
using System.Diagnostics;
//using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {

  class ThreadProgress : IDisposable {
    const int _1000 = 1000;
    readonly Action<ProgressMessage> _report;

    private int _valuePerMille;

    public ThreadProgress (Action<ProgressMessage> report) {
      _report = report;
    }

    public void Dispose () {
      //Log0 (3, this);
      int inc = _1000 - _valuePerMille;
      if (inc > 0) {
#if TRACE && EXTRA
        Trace.WriteLine ($"{this.GetType ().Name}.{nameof (Dispose)} {inc}");
#endif
        _report?.Invoke (new ProgressMessage {
        IncStepsPerMille = (uint)inc
        });
      }
    }

    public void Report (double value) {
      int val = (int)(value * _1000);
      int total = Math.Min (_1000, val);
      int inc = total - _valuePerMille;
      _valuePerMille = total;
      if (inc > 0) {
#if TRACE && EXTRA
        Trace.WriteLine ($"{this.GetType ().Name}.{nameof (Report)} {value:f3}->{inc}");
#endif
        _report?.Invoke (new ProgressMessage {
          IncStepsPerMille = (uint)inc
        });
      }
    }


  }

}
