using System;
using System.Windows.Forms;
using audiamus.aux;

namespace audiamus.aaxconv {
  partial class WhatsNewForm : Form {

    public WhatsNewForm () {
      InitializeComponent ();
    }


    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);

      try {
        var version = ApplEnv.AssemblyVersion;
        int fields = 2;
        if (version.Revision > 0)
          fields = 4;
        else if (version.Build > 0)
          fields = 3;

        Text = $"{Owner.Text}, Version {version.ToString (fields)} : {Text}";

        richTextBox1.Rtf = Properties.Resources.LatestRtf;
      } catch (Exception) {
      }
    }

  }

  
}
