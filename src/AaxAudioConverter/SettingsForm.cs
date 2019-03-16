using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using audiamus.aaxconv.lib;
using audiamus.aaxconv.lib.ex;
using audiamus.aux;
using audiamus.aux.ex;
using audiamus.aux.win;
using static audiamus.aux.ApplEnv;

namespace audiamus.aaxconv {
  using R = Properties.Resources;

  partial class SettingsForm : Form {

    readonly ISettings _settings = Properties.Settings.Default;
    readonly AaxAudioConverter _converter;
    readonly Func<InteractionMessage, bool?> _callback;

    public bool SettingsReset { get; private set; }

    private ISettings Settings => _settings;

    public SettingsForm (AaxAudioConverter converter, Func<InteractionMessage, bool?> callback) {
      InitializeComponent ();

      _converter = converter;
      _callback = callback;

      initControlsFromSettings ();
    }

    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);
      this.Text = $"{Owner?.Text}: {this.Text}";
    }

    protected override void OnKeyDown (KeyEventArgs e) {
      if (e.Modifiers == Keys.Control)
        switch (e.KeyCode) {
          case Keys.A:
            selectAll ();
            break;
          case Keys.C:
            copySelectionToClipboard ();
            break;
          default:
            base.OnKeyDown (e);
            break;
        } else
        base.OnKeyDown (e);
    }

    private void initControlsFromSettings () {
      txtBoxCustPart.Text = Settings.PartNames;
      txtBoxCustTitleChars.Text = Settings.AddnlValTitlePunct;
      ckBoxFileAssoc.Checked = Settings.FileAssoc ?? false;
      var codes = _converter.RegistryActivationCodes?.Select (c => c.ToHexDashString ()).ToArray ();
      if (!(codes is null))
        listBoxActCode.Items.AddRange (codes);

      cmBoxLang.SetCultures (typeof(MainForm), Settings);
    }


    private void selectAll () {
      for (int i = 0; i < listBoxActCode.Items.Count; i++)
        listBoxActCode.SetSelected (i, true);
    }

    private void copySelectionToClipboard () {
      try {
        var sb = new StringBuilder ();
        foreach (object row in listBoxActCode.SelectedItems) {
          if (row is string s) {
            if (string.IsNullOrWhiteSpace (s))
              continue;
            if (sb.Length > 0)
              sb.AppendLine ();
            sb.Append (s);
          }
        }
        Clipboard.SetData (DataFormats.Text, sb.ToString ());
      } catch (Exception) { }
    }

    private void txtBoxCustPart_Leave (object sender, EventArgs e) {
      Settings.PartNames = txtBoxCustPart.Text.SplitTrim (new char[] {' ', ';', ','}).Combine();
      txtBoxCustPart.Text = Settings.PartNames;
    }

    private static readonly Regex _rgxWord = new Regex (@"([\w\s])", RegexOptions.Compiled);

    private void txtBoxCustTitleChars_TextChanged (object sender, EventArgs e) {
      string s = txtBoxCustTitleChars.Text;
      var match = _rgxWord.Match (s);
      if (match.Success)
        s = s.Remove (match.Index, 1);

      char[] chars = s.ToCharArray ();
      chars = chars.Distinct ().ToArray();
      txtBoxCustTitleChars.Text = new string (chars);
      txtBoxCustTitleChars.SelectionStart = txtBoxCustTitleChars.Text.Length;
      txtBoxCustTitleChars.SelectionLength = 0;
    }


    private void txtBoxCustTitleChars_Leave (object sender, EventArgs e) {
      Settings.AddnlValTitlePunct = txtBoxCustTitleChars.Text;
    }


    private void btnUsrActCode_Click (object sender, EventArgs e) {
      new ActivationCodeForm () { Owner = this }.ShowDialog ();
    }

    private void btnRegActCode_Click (object sender, EventArgs e) {
      listBoxActCode.Visible = !listBoxActCode.Visible;
      btnRegActCode.Text = listBoxActCode.Visible ? R.CptHide : R.CptShow;
    }

    private void btnFfmpegLoc_Click (object sender, EventArgs e) {
      new FFmpegLocationForm (_converter, _callback) { Owner = this }.ShowDialog ();
    }

    private void btnReset_Click (object sender, EventArgs e) {
      if (MsgBox.Show (this, R.MsgResetAllSettings, 
        this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        return;
      if (MsgBox.Show (this, R.MsgAllModifLost, 
        this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
        return;

      DefaultSettings.ResetToDefault (Properties.Settings.Default);
      SettingsReset = true;
      initControlsFromSettings ();

    }

    private void btnOK_Click (object sender, EventArgs e) {

      bool ck = ckBoxFileAssoc.Checked;
      if ((Settings.FileAssoc ?? false) != ck) {
        Settings.FileAssoc = ck;
        new FileAssoc (Settings, this).Update ();
      }


      if (Culture.ChangeLanguage (cmBoxLang, Settings)) {
        Settings.Save ();

        if (MsgBox.Show (this, $"{ApplName} {R.MsgLangRestart}", Owner.Text, MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
          return;

        Application.Restart ();
        Environment.Exit (0);
      }

    }
  }
}
