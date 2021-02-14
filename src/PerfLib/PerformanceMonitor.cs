using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using audiamus.aux;
using audiamus.aux.ex;

namespace audiamus.perf {
  public class PerformanceMonitor : IProcessList, IDisposable {

    const int IVL_MS = 200;

    struct ProcessCpuUsage {
      public Process Process { get; }
      public CpuUsage Usage { get; }

      public ProcessCpuUsage (Process process) {
        Process = process;
        Usage = new CpuUsage (process.Id);
      }
    }

    public Action<IPerfCallback> Callback {
      private get => _callback;
      set
      {
        lock (_lockable)
          _callback = value;
      }
    }

    private Dictionary<int, CpuUsage> _processes =
      new Dictionary<int, CpuUsage> ();

    private readonly object _lockable = new object ();
    private readonly int _parentPID;
    private readonly string _childProcessName;
    private Timer _timer;
    private Action<IPerfCallback> _callback;
    private float _meanUsage;

    public PerformanceMonitor (string childProcessName = null) {
      var process = Process.GetCurrentProcess ();
      _parentPID = process.Id;
      (this as IProcessList).Add (process);

      _childProcessName = childProcessName;

      Singleton<ProcessList>.Instance.Notify = this;
    }

    public void Dispose () {
      Stop ();
    }

    public void Start () {
      if (_timer != null)
        return;
      _timer = new Timer (timerCallback, null, IVL_MS, IVL_MS);
    }


    public void Stop () {
      _timer?.Dispose ();
      _timer = null;
      _meanUsage = 0;

      lock (_lockable) {
        var processes = _processes.Keys.ToList ();
        processes.Remove (_parentPID);
        foreach (var p in processes)
          _processes.Remove (p);
      }

      Callback?.Invoke (new PerfCallback (
        new PerfCallback.ValueMax (0, ApplEnv.ProcessorCount),
        new PerfCallback.ValueMax (0)
      ));

    }

    private void timerCallback (object state) {
      IEnumerable<CpuUsage> processList;
      lock (_lockable) {
        if (Callback is null)
          return;
        processList = _processes.Values.ToList ();
      }

      float usage = 0;
      int cnt = 0;

      foreach (var proc in processList) {
        float? usg = proc.GetCurrentValue ();
        if (usg.HasValue) {
          if (usg.Value > 0)
            cnt++;
          usage += usg.Value;
        }
      }

      if (cnt > 0)
        usage /= cnt;
      usage = Math.Min (usage, 100f);

      const int F = 3;
      if (_meanUsage < 0)
        _meanUsage = (int)usage;
      else
        _meanUsage = ((_meanUsage * F) + (int)usage) / (F + 1);

      Callback?.Invoke (new PerfCallback (
        new PerfCallback.ValueMax (processList.Count() - 1, ApplEnv.ProcessorCount),
        new PerfCallback.ValueMax ((int)_meanUsage)
      ));

    }

    bool IProcessList.Add (Process process) {
      if (!_childProcessName.IsNullOrWhiteSpace () && !process.ProcessName.StartsWith (_childProcessName, StringComparison.InvariantCultureIgnoreCase))
        return false;
      lock (_lockable) {
        _processes[process.Id] = new CpuUsage (process.Id);
        Logging.Log (4, this, () => $"pid={process.Id}, #proc={_processes.Count}");
      }
      return true;
    }

    bool IProcessList.Remove (Process process) {
      lock (_lockable) {
        bool succ = _processes.Remove (process.Id);
        Logging.Log (4, this, () => $"pid={process.Id}, succ={succ}, #proc={_processes.Count}");
        return succ;
      }
    }

  }
}
