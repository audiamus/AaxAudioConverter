using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using audiamus.aux;
using static audiamus.aux.ApplEnv;

namespace audiamus.aaxconv {
  public partial class WhatsNewForm : Form {
    public WhatsNewForm () {
      InitializeComponent ();
    }

    protected override void OnActivated (EventArgs e) {
      base.OnActivated (e);

      try {
        Text = $"{Owner.Text}, Version {ApplEnv.AssemblyVersion.ToString (2)} : {Text}";


        const string HTML = ".html";
        const string LATEST = ".latest";

        string uiCultureName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
        string neutralHtmlFile = ApplName + LATEST + HTML;

        var sb = new StringBuilder ();
        sb.Append (ApplName + LATEST);
        if (uiCultureName != NeutralCultureName)
          sb.Append ($".{uiCultureName}");
        sb.Append (HTML);
        string localizedHtmlFile = sb.ToString ();

        var filename = Path.Combine (ApplDirectory, localizedHtmlFile);
        if (!File.Exists (filename))
          filename = Path.Combine (ApplDirectory, neutralHtmlFile);
        
        webBrowser.Url = new Uri (filename);
      } catch (Exception) {
      }
    }

  }

  
}
