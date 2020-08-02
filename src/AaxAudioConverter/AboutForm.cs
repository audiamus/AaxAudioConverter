using System;
using System.Diagnostics;
using System.Windows.Forms;
using static audiamus.aux.ApplEnv;

namespace audiamus.aaxconv {
  partial class AboutForm : Form {
    public AboutForm () {
      InitializeComponent ();
      Text += AssemblyTitle;

    }

    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);
      lblProduct.Text = AssemblyProduct;
      lblVersion.Text += AssemblyVersion;
      lblCopyright.Text += AssemblyCopyright;
    }

    private void linkLabelHomepage_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e) {
      var s = ((LinkLabel)sender).Text;
      Process.Start (s);
    }
  }



}
