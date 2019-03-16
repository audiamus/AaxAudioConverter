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
    static void Main () {
      using (Mutex mutex = new Mutex (false, ApplEnv.AssemblyGuid)) {

        if (!mutex.WaitOne (0, false)) 
          return;

        Application.EnableVisualStyles ();
        Application.SetCompatibleTextRenderingDefault (false);

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Culture.Init (Properties.Settings.Default);

        Application.Run (new MainForm ());
      }
    }
  }
}
