using System;
using System.Globalization;
using System.Windows.Forms;
using audiamus.aux.win;

namespace audiamus.aaxconv {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main () {
      Application.EnableVisualStyles ();
      Application.SetCompatibleTextRenderingDefault (false);

      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      Culture.Init (Properties.Settings.Default);

      Application.Run (new MainForm ());
    }
  }
}
