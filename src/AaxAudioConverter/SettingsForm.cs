using System;
using System.ComponentModel;
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

    private readonly ISettings _settings = Properties.Settings.Default;
    private readonly AaxAudioConverter _converter;
    private readonly Func<InteractionMessage, bool?> _callback;
    private bool _flag;

    public bool SettingsReset { get; private set; }

    private ISettings Settings => _settings;

    public SettingsForm (AaxAudioConverter converter, Func<InteractionMessage, bool?> callback) {
      using (new ResourceGuard (x => _flag = x))
        InitializeComponent ();

      _converter = converter;
      _callback = callback;

      initPartNaming ();
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

    private void initPartNaming () {
      var rm = this.GetDefaultResourceManager ();
      var enums = EnumUtil.GetValues<EGeneralNaming> ();
      var data = enums.Select (e => e.ToDisplayString<EGeneralNaming, ChainPunctuationBracket> (rm)).ToArray ();
      using (new ResourceGuard (x => _flag = x))
        comBoxPartName.DataSource = data;
      txtBoxPartName.DataBindings.Add (nameof (txtBoxPartName.Text), Settings, nameof (Settings.PartName));
    }


    private void initControlsFromSettings () {
      txtBoxCustPart.Text = Settings.PartNames;
      txtBoxCustTitleChars.Text = Settings.AddnlValTitlePunct;
      ckBoxFileAssoc.Checked = Settings.FileAssoc ?? false;
      using (new ResourceGuard (x => _flag = x))
        comBoxPartName.SelectedIndex = (int)Settings.PartNaming;
      txtBoxPartName.Enabled = Settings.PartNaming == EGeneralNaming.custom;

      var codes = _converter.RegistryActivationCodes?.Select (c => c.ToHexDashString ()).ToArray ();
      if (!(codes is null))
        listBoxActCode.Items.AddRange (codes);

      comBoxLang.SetCultures (typeof(MainForm), Settings);

      switch (Settings.OnlineUpdate) {
        case null:
          comBoxUpdate.SelectedIndex = 0;
          break;
        case false:
          comBoxUpdate.SelectedIndex = 1;
          break;
        case true:
          comBoxUpdate.SelectedIndex = 2;
          break;
      }
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

    private static readonly Regex _rgxWord = new Regex (@"[\w\s]", RegexOptions.Compiled);

    private void txtBoxCustTitleChars_TextChanged (object sender, EventArgs e) {
      if (_flag)
        return;

      string s = txtBoxCustTitleChars.Text;
      var match = _rgxWord.Match (s);
      if (match.Success)
        s = s.Remove (match.Index, 1);

      char[] chars = s.ToCharArray ();
      chars = chars.Distinct ().ToArray();
      using (new ResourceGuard (x => _flag = x)) 
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

      switch (comBoxUpdate.SelectedIndex) {
        case 0:
          Settings.OnlineUpdate = null;
          break;
        case 1:
          Settings.OnlineUpdate = false;
          break;
        case 2:
          Settings.OnlineUpdate = true;
          break;
      }



      if (Culture.ChangeLanguage (comBoxLang, Settings)) {
        Settings.Save ();

        if (MsgBox.Show (this, $"{ApplName} {R.MsgLangRestart}", Owner.Text, MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
          return;

        try {
          Application.Restart ();
        } catch (Exception) { }

        Environment.Exit (0);
      }

    }

    private void comBoxPartName_SelectedIndexChanged (object sender, EventArgs e) {
      if (_flag)
        return;
      Settings.PartNaming = (EGeneralNaming)comBoxPartName.SelectedIndex;
      txtBoxPartName.Enabled = Settings.PartNaming == EGeneralNaming.custom;
    }
  }
}
