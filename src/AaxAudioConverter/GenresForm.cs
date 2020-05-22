using System;
using System.Linq;
using System.Windows.Forms;
using audiamus.aux.ex;
using audiamus.aux.win;

namespace audiamus.aaxconv {
  using R = Properties.Resources;

  partial class GenresForm : Form {
    private readonly lib.IConvSettings _settings;

    private lib.IConvSettings Settings => _settings;

    public GenresForm (lib.IConvSettings settings) {
      InitializeComponent ();
      _settings = settings;

      var names = Settings.Genres.SplitTrim (';');
      listBox.Items.AddRange (names);
    }

    private void listBox_SelectedIndexChanged (object sender, EventArgs e) => 
      btnOK.Enabled = listBox.SelectedIndices.Count > 0;

    private void btnOK_Click (object sender, EventArgs e) {
      if (listBox.SelectedIndices.Count > 0) {
        var result = MsgBox.Show (this, R.MsgRemoveGenres, Owner.Text, MessageBoxButtons.YesNo, 
          MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

        if (result == DialogResult.Yes) {
          var names = Settings.Genres.SplitTrim (';').ToList ();
          foreach (int idx in listBox.SelectedIndices) {
            string name = (string)listBox.Items[idx];
            names.Remove (name);
          }
          Settings.Genres = names.Combine ();
        }
      }
      Close ();
    }
  }
}
