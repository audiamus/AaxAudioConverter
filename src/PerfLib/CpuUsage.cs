using System;
using System.Diagnostics;
using static audiamus.perf.ProcessCpuCounter;

namespace audiamus.perf {
  public class CpuUsage {
    private const float SAMPLE_FREQUENCY_MILLIS = 200; //1000;

    public const string PROCESSOR = "Processor";
    public const string TOTAL = "_Total";

    private readonly object _lockable = new object ();
    private PerformanceCounter _counter;
    private float? _lastSample;
    private DateTime _lastSampleTime;

    public CpuUsage (int pid = 0) {
      try {
        if (pid == 0)
          this._counter = new PerformanceCounter (PROCESSOR, PROCESSOR_TIME, TOTAL, true);
        else
          this._counter = GetPerfCounterForProcessId (pid);

        _counter?.NextValue ();
      } catch (Exception) { }
    }

    public float? GetCurrentValue () {
      if (_counter is null)
        return null;
      if ((DateTime.UtcNow - _lastSampleTime).TotalMilliseconds > SAMPLE_FREQUENCY_MILLIS) {
        lock (_lockable) {
          if ((DateTime.UtcNow - _lastSampleTime).TotalMilliseconds > SAMPLE_FREQUENCY_MILLIS) {
            try {
              _lastSample = _counter.NextValue ();
            } catch (Exception) {
              _lastSample = null;
            }
            _lastSampleTime = DateTime.UtcNow;
          }
        }
      }

      return _lastSample;
    }
  }

}
