using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using audiamus.aaxconv.lib;
using audiamus.aux;
using audiamus.aux.ex;
using audiamus.aux.win;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv {
  using R = Properties.Resources;

  partial class PreviewForm : FileItemForm {
    #region Private Fields

    // regex char class substraction, allow several non-word chars
    private static readonly Regex _rgxNonWord = new Regex (@"[\W-[\s\-\[\]\(\)<>{}/+,._&:]]", RegexOptions.Compiled);

    AaxFileItem _fileItem;
    bool _flag;

    #endregion Private Fields 


    #region Public Constructors

    public PreviewForm () {
      using (new ResourceGuard (x => _flag = x))
        InitializeComponent ();

    }

    #endregion Public Constructors


    #region Public Methods

    public override void Set (AaxFileItem fileItem) {

      if (hasCustomization ()) {
        var result = MsgBox.Show (
          this, R.MsgPreviewUnsavedChanges, this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
        switch (result) {
          case DialogResult.Cancel:
            return;
          case DialogResult.Yes:
            saveChanges ();
            break;
        }
      }
      
      _fileItem = fileItem;
      updatePreview (false);
    }

    public void UpdatePreview () => updatePreview (true);

    #endregion Public Methods

    #region Protected Methods

    protected override void OnResize (EventArgs e) {
      base.OnResize (e);
      layoutTable ();
    }

    #endregion Protected Methods

    #region Private Methods

    private void updatePreview (bool update) {
      if (_fileItem is null)
        return;

      using (new ResourceGuard (x => _flag = x)) {
        setAuthor (update);
        setTitle (update);
        setYear (update);
        setGenre (update);
      }

      layoutTable ();
      enableOk ();
    }

    private void saveChanges () {
      if (hasCustomization ()) {
        if (_fileItem.CustomNames is null)
          _fileItem.CustomNames = new CustomTagFileNames ();

        var cn = _fileItem.CustomNames;
        cn.AuthorTag = customized (txtBoxAuthorTag);
        cn.AuthorFile = customized (txtBoxAuthorFile);
        cn.TitleTag = customized (txtBoxTitleTag);
        cn.TitleFile = customized (txtBoxTitleFile);
        cn.YearTag = customized (numUpDnYearTag);
        cn.GenreTag = customized (comBoxGenreTag);

        Log (3, this, () => $"custom names:{Environment.NewLine}{cn}");

      } else {
        _fileItem.CustomNames = null;
        Log (3, this, "no customization");
      }
    }

    private void setAuthor (bool update) {
      if (update)
        return;

      txtBoxAuthorSource.Text = _fileItem.Author;
      txtBoxAuthorTag.Text = _fileItem.Author; // without narrator
      txtBoxAuthorFile.Text = _fileItem.Author.Prune ();

      txtBoxAuthorTag.Tag = txtBoxAuthorTag.Text;
      txtBoxAuthorFile.Tag = txtBoxAuthorFile.Text;

      if (_fileItem.CustomNames is null)
        return;
      if (!string.IsNullOrWhiteSpace (_fileItem.CustomNames.AuthorTag))
        txtBoxAuthorTag.Text = _fileItem.CustomNames.AuthorTag;
      if (!string.IsNullOrWhiteSpace (_fileItem.CustomNames.AuthorFile))
        txtBoxAuthorFile.Text = _fileItem.CustomNames.AuthorFile;
    }

    private void setGenre (bool update) {

      txtBoxGenreSource.Text = _fileItem.Genre;

      string text = Previewer.GetGenre (_fileItem);
      if (!update || !isCustomized (comBoxGenreTag))
        comBoxGenreTag.Text = text;

      var settings = Previewer.Settings;
      string[] genres = settings.Genres.SplitTrim (';');
      comBoxGenreTag.Items.Clear ();
      comBoxGenreTag.Items.AddRange (genres);

      comBoxGenreTag.Tag = text;

      if (_fileItem.CustomNames is null)
        return;
      if (!string.IsNullOrWhiteSpace (_fileItem.CustomNames.GenreTag))
        comBoxGenreTag.Text = _fileItem.CustomNames.GenreTag;

    }

    private void setTitle (bool update) {
      txtBoxTitleSource.Text = _fileItem.BookTitle;
      var (tag, file) = Previewer.TitlePreview (_fileItem);

      if (!update || !isCustomized (txtBoxTitleTag))
        txtBoxTitleTag.Text = tag;
      txtBoxTitleTag.Tag = tag;


      if (!update || !isCustomized (txtBoxTitleFile)) // && !isCustomized(txtBoxTitleTag)))
        txtBoxTitleFile.Text = file;
      txtBoxTitleFile.Tag = file;

      if (_fileItem.CustomNames is null)
        return;
      if (!string.IsNullOrWhiteSpace (_fileItem.CustomNames.TitleTag))
        txtBoxTitleTag.Text = _fileItem.CustomNames.TitleTag;
      if (!string.IsNullOrWhiteSpace (_fileItem.CustomNames.TitleFile))
        txtBoxTitleFile.Text = _fileItem.CustomNames.TitleFile;

    }

    private void setYear (bool update) {
      if (update)
        return;

      txtBoxYearSource.Text = _fileItem.PublishingDate?.Year.ToString ();
      numUpDnYearTag.Value = Math.Min (_fileItem.PublishingDate?.Year ?? 0, numUpDnYearTag.Maximum);

      numUpDnYearTag.Tag = numUpDnYearTag.Value;

      if (_fileItem.CustomNames is null)
        return;
      if (_fileItem.CustomNames.YearTag.HasValue)
        numUpDnYearTag.Value = _fileItem.CustomNames.YearTag.Value.Year;

    }

    private bool hasCustomization () {
      foreach (Control control in table.Controls)
        if (isCustomized (control))
          return true;
      return false;
    }

    private static bool isCustomized (Control control) {
      if (control.Tag is null)
        return false;
      if (control is NumericUpDown nud)
        return nud.Value != (decimal)nud.Tag;
      else
        return control.Text != (string)control.Tag;
    }

    private static string customized (Control control) {
      if (isCustomized (control)) {
        return control.Text.Trim();
      } else
        return null;
    }

    private static DateTime? customized (NumericUpDown control) {
      if (isCustomized (control)) {
        return new DateTime ((int)control.Value, 1, 1);
      } else
        return null;
    }

    private void adjustFile (TextBox txtBoxTag, TextBox txtBoxFile) {
      if (isCustomized (txtBoxFile))
        return;

      string text = txtBoxTag.Text;
      if (txtBoxFile == txtBoxTitleFile)
        text = Previewer.PruneTitle (text);
      else
        text = text.Prune ();
      txtBoxFile.Text = text;
      //txtBoxFile.Tag = text; // better not
    }


    private void checkGenreText () {
      string s = comBoxGenreTag.Text;
      var match = _rgxNonWord.Match (s);
      if (!match.Success)
        return;

      s = s.Remove (match.Index, 1);
      using (new ResourceGuard (x => _flag = x))
        comBoxGenreTag.Text = s;
    }


    private void updateGenreNames () {
      if (!isCustomized (comBoxGenreTag))
        return;
      string value = comBoxGenreTag.Text;
      var settings = Previewer.Settings;
      var names = settings.Genres.SplitTrim (';').ToList ();
      if (names.Contains (value))
        return;
      names.Add (value);
      names.Sort ();
      settings.Genres = names.Combine ();
      comBoxGenreTag.Items.Clear ();
      comBoxGenreTag.Items.AddRange (names.ToArray());
    }


    private void enableOk () {
      bool isDirty = hasCustomization ();
      bool wasDirty = _fileItem?.CustomNames != null;
      btnOK.Enabled = isDirty || _fileItem?.CustomNames != null;
      if (wasDirty) {
        if (isDirty)
          lblSts.Text = R.StsHasCustomization;
        else
          lblSts.Text = R.StsNoCustomizationOk;
      } else {
        if (isDirty)
          lblSts.Text = R.StsHasCustomization;
        else
          lblSts.Text = R.StsNoCustomization;
      }
    }

    private void layoutTable () {
      if (_flag)
        return;

      int nClms = table.ColumnCount;
      int nRows = table.RowCount;

      List<int> widths = new List<int> ();
      for (int i = 0; i < nClms; i++) {
        int maxWidth = 0;

        for (int j = 0; j < nRows; j++) {
          var control = table.GetControlFromPosition (i, j);
          if (control is null)
            continue;

          int wid = 0;
          switch (control) {
            case NumericUpDown nud:
              wid = TextRenderer.MeasureText ("0000", control.Font).Width + 20;
              break;
            case Button btn:
              wid = btn.Width;
              break;
            default:
              wid = TextRenderer.MeasureText (control.Text, control.Font).Width;
              if (control is ComboBox)
                wid += 20;
              break;
          }
          if (wid > maxWidth)
            maxWidth = wid;
        }

        maxWidth += 10; // allow for border

        widths.Add (maxWidth);
      }
           
      int totalWidth = table.ClientSize.Width;

      // column 0 (header) and 3 (year) should be absolute size, split the rest proportionally
      int absWidth = 0;
      for (int i = 0; i < nClms; i++) {
        ColumnStyle style = table.ColumnStyles[i];
        if (style.SizeType == SizeType.Absolute)
          absWidth += widths[i];
      }

      int percWidth = totalWidth - absWidth;
      // column 0 (header) and 3 (year) should be absolute size, split the rest proportionally
      for (int i = 0; i < nClms; i++) {
        ColumnStyle style = table.ColumnStyles[i];
        if (style.SizeType == SizeType.Percent)
          widths[i] = adjustWidth (widths[i], percWidth);
      }

      using (new ResourceGuard (x => { if (x) this.SuspendLayout (); else this.ResumeLayout (); })) {
        for (int i = 0; i < nClms; i++) {
          ColumnStyle style = table.ColumnStyles[i];
          style.Width = widths[i];
        }
      }
    }

    private int adjustWidth (int wid, int percWidth) => wid * 100 / percWidth;
    
    #endregion Private Methods

    #region Event Handlers
    private void btnOK_Click (object sender, EventArgs e) {
      saveChanges ();
      DialogResult = DialogResult.OK;
      Close ();
    }

    private void btnCancel_Click (object sender, EventArgs e) => Close ();

    private void btnGenres_Click (object sender, EventArgs e) {
      GenresForm dlg = new GenresForm (Previewer.Settings) { Owner = this };
      dlg.ShowDialog ();
    }

    private void txtBox_TextChanged (object sender, EventArgs e) {
      if (_flag)
        return;

      if (sender is Control ctl) {
        if (ctl is NumericUpDown nud) {
          if (nud.Value == 0)
            nud.Value = (decimal)ctl.Tag;
        } else {
          if (sender is TextBox tb) {
            if (tb == txtBoxAuthorFile || tb == txtBoxTitleFile) {
              int i = tb.SelectionStart;
              int l0 = tb.Text.Length;
              using (new ResourceGuard (x => _flag = x))
                ctl.Text = ctl.Text.Prune ();
              int l = tb.Text.Length;
              if (l < l0) {
                tb.SelectionLength = 0;
                tb.SelectionStart = i >= l ? l : 0;
              }
            }
          }

          if (string.IsNullOrWhiteSpace (ctl.Text))
            ctl.Text = (string)ctl.Tag;
        }
      }

      if (sender == comBoxGenreTag)
        checkGenreText ();


      enableOk ();
    }

    private void txtBox_Leave (object sender, EventArgs e) {
      layoutTable ();

      foreach (Control control in table.Controls) {
        // left adjust
        if (control is TextBox tb) {
          tb.SelectionStart = 0;
          tb.SelectionLength = 0;
        }
      }

      // update file 
      if (sender == txtBoxAuthorTag)
        adjustFile (txtBoxAuthorTag, txtBoxAuthorFile);
      else if (sender == txtBoxTitleTag)
        adjustFile (txtBoxTitleTag, txtBoxTitleFile);
      if (sender == comBoxGenreTag)
        updateGenreNames ();

      enableOk ();
    }

    private void numUpDnYearTag_Validating (object sender, System.ComponentModel.CancelEventArgs e) {
      if (string.IsNullOrWhiteSpace (numUpDnYearTag.Text))
        numUpDnYearTag.Text = ((decimal)numUpDnYearTag.Tag).ToString();
    }

    #endregion Event Handlers
  }
}
