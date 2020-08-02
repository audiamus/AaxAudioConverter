using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace audiamus.aaxconv {
  partial class StartupTipForm : Form {
    private readonly IAppSettings _settings;

    public StartupTipForm (IAppSettings settings) {
      InitializeComponent ();
      _settings = settings;
      if (_settings.ShowStartupTip.HasValue)
        ckBoxDisable.Checked = !_settings.ShowStartupTip.Value;

    }


    protected override void OnLoad (EventArgs e) {
      Location = new Point (Owner.Location.X + (Owner.Width - Width) / 2, Owner.Location.Y + (Owner.Height - Height) / 2);
      base.OnLoad (e);
    }

    protected override void OnClosing (CancelEventArgs e) {
      base.OnClosing (e);
      _settings.ShowStartupTip = !ckBoxDisable.Checked;
    }

    private void btnOK_Click (object sender, EventArgs e) => Close ();
  }

  
}
