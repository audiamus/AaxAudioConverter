using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace audiamus.aux {
  public class ProcessHost {

    protected Process Process { get; private set; }

    /// <summary>
    /// Executes a process and passes its command-line output back after the process has exited
    /// </summary>
    protected string runProcess (
      string exePath, string parameters, 
      bool getStdErrorNotOutput = false,
      DataReceivedEventHandler eventHandler = null) 
    {

      if (!File.Exists (exePath))
        return null;

      string result = String.Empty;
      bool async = eventHandler != null;

      using (Process p = new Process ()) {
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.FileName = exePath;
        p.StartInfo.Arguments = parameters;

        if (getStdErrorNotOutput)
          p.StartInfo.RedirectStandardError = true;
        else
          p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;

        if (async) {
          if (getStdErrorNotOutput)
            p.ErrorDataReceived += eventHandler;
          else
            p.OutputDataReceived += eventHandler;
        }

        var processOutputStringBuilder = new StringBuilder ();

        p.Start ();
        p.PriorityClass = ProcessPriorityClass.BelowNormal;

        Singleton<ProcessList>.Instance.Add (p);
        Process = p;

        if (async) {
          if (getStdErrorNotOutput)
            p.BeginErrorReadLine ();
          else
            p.BeginOutputReadLine ();
        }
        p.WaitForExit ();

        Singleton<ProcessList>.Instance.Remove (p);
        Process = null;

        if (!async) {
          if (getStdErrorNotOutput)
            result = p.StandardError.ReadToEnd ();
          else
            result = p.StandardOutput.ReadToEnd ();
        } else {
          if (processOutputStringBuilder.Length > 0)
            result = processOutputStringBuilder.ToString ();
        }

      }

      return result;
    }
  }

}
