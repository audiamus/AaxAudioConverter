using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using audiamus.aaxconv.lib.ex;
using audiamus.aux.win;

namespace audiamus.aaxconv {

  using R = Properties.Resources;

  partial class ActivationCodeForm : Form {
    readonly ISettings _settings = Properties.Settings.Default;
    readonly bool _suppressMsgBox;
    uint? _code;

    static readonly Regex _rgx = new Regex (@"^(([a-fA-F0-9]{2})\W?){3}([a-fA-F0-9]{2})$", RegexOptions.Compiled);

    private ISettings Settings => _settings;

    public ActivationCodeForm (bool suppressMsgBox = false) {
      InitializeComponent ();

      _suppressMsgBox = suppressMsgBox;

      textBox1.Text = Settings.ActivationCode.ToHexDashString ();
      
    }

    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);
      //if (!(Owner is null))
      //  this.Text = $"{Owner.Text} : {this.Text}";
    }

    private void textBox1_TextChanged (object sender, EventArgs e) {
      bool succ = false;
      if (string.IsNullOrEmpty (textBox1.Text) && Settings.ActivationCode.HasValue) {
        _code = null;
        succ = true;
      } else {
        //group 2, 4 captures
        var match = _rgx.Match (textBox1.Text);

        succ = match.Success;
        if (succ && match.Groups.Count == 4) {
          var caps = match.Groups[2].Captures;
          var chars = caps.Cast<Capture> ().Select (c => c.Value).ToList ();
          chars.Add (match.Groups[3].Value);
          _code = chars.ToUInt32 ();
        }
      }
      btnOk.Enabled = succ;
      AcceptButton = btnOk.Enabled ? btnOk : btnCancel;
    }

    private void btnOk_Click (object sender, EventArgs e) {
      if (Settings.ActivationCode != _code && !_suppressMsgBox) {
        if (_code.HasValue)
          MsgBox.Show (this, R.MsgNoteActivationCode, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
          MsgBox.Show (this, R.MsgActivationCodeRemoved, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      Settings.ActivationCode = _code;

      DialogResult = DialogResult.OK;
      Close ();
    }
  }
}
