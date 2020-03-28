using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using audiamus.aux;
using audiamus.aux.win;

namespace audiamus.aaxconv {
  static class Program {

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main (string[] args) {
      using (Mutex mutex = new Mutex (false, ApplEnv.AssemblyGuid)) {

        if (!mutex.WaitOne (0, false)) 
          return;

        var ap = new ArgParser (args);
        uint? loglevel = ap.FindUIntArg ("Log");
        if (loglevel.HasValue) {
          Logging.InstantFlush = true;
          Logging.Level = (int)loglevel;
        }

        Application.EnableVisualStyles ();
        Application.SetCompatibleTextRenderingDefault (false);

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Culture.Init (Properties.Settings.Default);

        Application.Run (new MainForm ());
      }
    }
  }
}
