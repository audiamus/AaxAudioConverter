using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace audiamus.perf {
  public class ProcessCpuCounter {
    public const string PROCESSOR_TIME = "% Processor Time";
    public const string PROCESS = "Process";
    public const string ID_PROCESS = "ID Process";

    public static PerformanceCounter GetPerfCounterForProcessId (int processId, string processCounterName = PROCESSOR_TIME) {
      string instance = GetInstanceNameForProcessId (processId);
      if (string.IsNullOrEmpty (instance))
        return null;

      return new PerformanceCounter (PROCESS, processCounterName, instance);
    }

    public static string GetInstanceNameForProcessId (int processId) {
      try {
        var process = Process.GetProcessById (processId);
        string processName = Path.GetFileNameWithoutExtension (process.ProcessName);

        PerformanceCounterCategory cat = new PerformanceCounterCategory (PROCESS);
        string[] instances = cat.GetInstanceNames ()
            .Where (inst => inst.StartsWith (processName))
            .ToArray ();

        foreach (string instance in instances) {
          using (PerformanceCounter cnt = new PerformanceCounter (PROCESS, ID_PROCESS, instance, true)) {
            int val = (int)cnt.RawValue;
            if (val == processId)
              return instance;
          }
        }
      } catch (Exception) { }
      return null;
    }
  }
}
