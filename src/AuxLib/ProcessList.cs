using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace audiamus.aux {
  public class ProcessList : IDisposable {

    private readonly object _lockable = new object ();
    private bool _disposed = false;

    HashSet<Process> _processes = new HashSet<Process> ();

    public bool AddProcess (Process process) {
      lock (_lockable)
        return _processes.Add (process);
    }

    public bool RemoveProcess (Process process) {
      lock (_lockable)
        return _processes.Remove (process);
    }

    #region IDisposable Members

    public void Dispose () {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    #endregion
    private void Dispose (bool disposing) {
      if (_disposed)
        return;

      if (disposing) {
      }

      lock (_lockable)
        foreach (Process p in _processes) 
          p.Kill ();
      
      _disposed = true;
    }
  }
}
