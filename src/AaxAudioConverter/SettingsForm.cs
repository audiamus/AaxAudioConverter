using System;
using System.Drawing;
using System.IO;
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
    public const string PART = "Part";

    private readonly IAppSettings _settings = Properties.Settings.Default;
    private readonly AaxAudioConverter _converter;
    private readonly Func<InteractionMessage, bool?> _callback;
    private bool _flag;
    private bool _enabled = true;
    private readonly string _title; 
    private bool _flagResetUpdateOthers;

    private ComboBoxEnumAdapter<EAaxCopyMode> _cbAdapterAaxCopyMode;

    public bool SettingsReset { get; private set; }
    public bool Dirty { get; private set; }
    public bool ListViewDirty { get; private set; }

    public new bool Enabled {
      get => _enabled;
      set
      {
        if (value == Enabled)
          return;
        _enabled = value;
        tabControl1.Enabled = _enabled;
        tabControl1.DrawMode = _enabled ? TabDrawMode.Normal : TabDrawMode.OwnerDrawFixed;
        btnReset.Enabled = _enabled;
        btnOK.Enabled = _enabled;
      }
    }

    private IAppSettings Settings => _settings;

    public SettingsForm (AaxAudioConverter converter, Func<InteractionMessage, bool?> callback) {
      using (new ResourceGuard (x => _flag = x))
        InitializeComponent ();

      _title = this.Text;
      _converter = converter;
      _callback = callback;

      initPartNaming ();
      initFlatFoldersNaming ();
      initReducedBitrate ();
      initControlsFromSettings ();
    }


    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);
      this.Text = $"{Owner?.Text}: {_title}";
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
      var rm = R.ResourceManager; // this.GetDefaultResourceManager ();
      var enums = EnumUtil.GetValues<EGeneralNaming> ();
      var data = enums.Select (e => e.ToDisplayString<EGeneralNaming, ChainPunctuationBracket> (rm)).ToArray ();
      using (new ResourceGuard (x => _flag = x))
        comBoxPartName.DataSource = data;
      //txtBoxPartName.DataBindings.Add (nameof (txtBoxPartName.Text), Settings, nameof (Settings.PartName));
      txtBoxPartName.Text = Settings.PartName;
    }

    private void initReducedBitrate () {
      var bitrates = EnumUtil.GetValues<EReducedBitRate> ();
      string defval = comBoxRedBitRate.Items[0] as string;
      comBoxRedBitRate.Items.Clear ();
      foreach (var ebitrate in bitrates) {
        uint bitrate = ebitrate.UInt32 ();
        if (bitrate == 0)
          comBoxRedBitRate.Items.Add (defval);
        else {
          string s = $"{bitrate} kb/s max";
          comBoxRedBitRate.Items.Add (s);
        }
      }
    }

    private void updatePartNaming () {
      var rm = R.ResourceManager; // this.GetDefaultResourceManager ();
      var partNaming = (EGeneralNaming)comBoxPartName.SelectedIndex;
      string standardPrefix = rm.GetStringEx (PART);
      using (new ResourceGuard (x => _flag = x))
        if (partNaming != EGeneralNaming.custom)
          txtBoxPartName.Text = standardPrefix;
        else
          txtBoxPartName.Text = string.IsNullOrWhiteSpace (Settings.PartName) ? standardPrefix : Settings.PartName;
    }

    private void initFlatFoldersNaming () {
      var rm = this.GetDefaultResourceManager ();
      var enums = EnumUtil.GetValues<EFlatFolderNaming> ();
      var data = enums.Select (e => e.ToDisplayString<EFlatFolderNaming, ChainPunctuationDash> (rm)).ToArray ();
      using (new ResourceGuard (x => _flag = x))
        comBoxFlatFolders.DataSource = data;
    }

    private void initControlsFromSettings () {
      tabControl1.SelectedIndex = Settings.SettingsTab;

      initControlsFromSettingsGeneral ();
      initControlsFromSettingsFolder ();
      initControlsFromSettingsConversion ();
      initControlsFromSettingsChapters ();
      intoControlsFromSettingsMetaTags ();
    }

    private void initControlsFromSettingsGeneral () {
      // tab page General

      ckBoxFfmpegVersCheck.Checked = Settings.RelaxedFFmpegVersionCheck;
      
      var codes = _converter.NumericActivationCodes?.Select (c => c.ToHexDashString ()).ToArray ();
      if (!(codes is null))
        listBoxActCode.Items.AddRange (codes);

      ckBoxFileAssoc.Checked = Settings.FileAssoc ?? false;

      ckBoxDateClm.Checked = Settings.FileDateColumn;

      btnAaxCopyDir.Enabled = Settings.AaxCopyMode != default;
      using (new ResourceGuard (x => _flag = x))
        _cbAdapterAaxCopyMode =
          new ComboBoxEnumAdapter<EAaxCopyMode> (comBoxAaxCopy, this.GetDefaultResourceManager (), Settings.AaxCopyMode);

      ckBoxLaunchPlayer.Checked = Settings.AutoLaunchPlayer;

      comBoxUpdate.SelectedIndex = (int)Settings.OnlineUpdate;

      comBoxLang.SetCultures (typeof (MainForm), Settings);
    }

    private void initControlsFromSettingsFolder () {
      // tab page Folder

      ckBoxFlatFolders.Checked = Settings.FlatFolders;      
      enableFlatFoldersDependencies (Settings.FlatFolders);
      using (new ResourceGuard (x => _flag = x))
        comBoxFlatFolders.SelectedIndex = (int)Settings.FlatFolderNaming;

      ckBoxSeries.Checked = Settings.WithSeriesTitle;
      panelSeriesDigits.Enabled = Settings.WithSeriesTitle;
      numUpDnSeriesDigits.Value = Settings.NumDigitsSeriesSeqNo;

      ckBoxFullCaptionBookFolder.Checked = Settings.FullCaptionBookFolder;

      using (new ResourceGuard (x => _flag = x))
        comBoxPartName.SelectedIndex = (int)Settings.PartNaming;
      txtBoxPartName.Enabled = Settings.PartNaming == EGeneralNaming.custom;    
      updatePartNaming ();

      using (new ResourceGuard (x => _flag = x))
        comBoxOutFolderConflict.SelectedIndex = (int)Settings.OutFolderConflict;

    }

    private void initControlsFromSettingsConversion () {

      // tab page Conversion

      txtBoxCustPart.Text = Settings.PartNames;
      
      txtBoxCustTitleChars.Text = Settings.AddnlValTitlePunct;
      
      ckBoxIntermedCopySingle.Checked = Settings.IntermedCopySingle;
      
      using (new ResourceGuard (x => _flag = x))
        comBoxFixAacEncoding.SelectedIndex = (int)Settings.FixAACEncoding;

      ckBoxVarBitRate.Checked = Settings.VariableBitRate;
      
      using (new ResourceGuard (x => _flag = x))
        comBoxRedBitRate.SelectedIndex = (int)Settings.ReducedBitRate;

      comBoxM4B.SelectedIndex = Settings.M4B ? 1 : 0;

      ckBoxLatin1.Checked = Settings.Latin1EncodingForPlaylist;
      
      ckBoxExtraMetaFiles.Checked = Settings.ExtraMetaFiles;
    }

    private void initControlsFromSettingsChapters () {
      // tab page Chapters

      comBoxNamedChapters.SelectedIndex = (int)Settings.NamedChapters;
      enablePreferEmbeddedChapterTimes ();
      
      nudShortChapter.Value = Settings.ShortChapterSec;
      nudVeryShortChapter.Value = Settings.VeryShortChapterSec;

      comBoxVerAdjChapters.SelectedIndex = (int)Settings.VerifyAdjustChapterMarks;
      comBoxPrefEmbChapTimes.SelectedIndex = (int)Settings.PreferEmbeddedChapterTimes;
    }

    private void intoControlsFromSettingsMetaTags () {
      // tab page Tags

      comBoxArtist.SelectedIndex = indexOfRole (Settings.TagArtist);
      comBoxAlbumArtist.SelectedIndex = indexOfRole (Settings.TagAlbumArtist);
      comBoxComposer.SelectedIndex = indexOfRole (Settings.TagComposer);
      comBoxConductor.SelectedIndex = indexOfRole (Settings.TagConductor);

      Settings.Narrator = null;
    }

    private int indexOfRole (ERoleTagAssignment role) {
      bool narrator = Settings.Narrator ?? false;
      switch (role) {
        default:
        case ERoleTagAssignment.none:
          return 0;
        case ERoleTagAssignment.author:
          return 1;
        case ERoleTagAssignment.author__narrator__:
          if (narrator)
            return 2;
          else
            return 1;
        case ERoleTagAssignment.author_narrator:
          return 2;
        case ERoleTagAssignment.__narrator__:
          if (narrator)
            return 3;
          else
            return 0;
        case ERoleTagAssignment.narrator:
          return 3;
      }
    }

    private ERoleTagAssignment roleOfIndex (int index) {
      switch (index) {
        default:
        case 0: 
          return ERoleTagAssignment.none;
        case 1:
          return ERoleTagAssignment.author;
        case 2:
          return ERoleTagAssignment.author_narrator;
        case 3:
          return ERoleTagAssignment.narrator;
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
      string partNames = txtBoxCustPart.Text.SplitTrim (new char[] {' ', ';', ','}).Combine();
      txtBoxCustPart.Text = partNames;
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


    private void btnUsrActCode_Click (object sender, EventArgs e) {
      var result = new ActivationCodeForm () { Owner = this }.ShowDialog ();
      if (result == DialogResult.OK)
        _converter.ReinitActivationCode ();
    }

    private void btnRegActCode_Click (object sender, EventArgs e) {
      listBoxActCode.Visible = !listBoxActCode.Visible;
      btnRegActCode.Text = listBoxActCode.Visible ? R.CptHide : R.CptShow;
    }

    private void btnFfmpegLoc_Click (object sender, EventArgs e) {
      string oldSetting = _settings.FFMpegDirectory;
      var dlg = new FFmpegLocationForm (_converter, _callback) { Owner = this };
      dlg.ShowDialog ();
      string newSetting = _settings.FFMpegDirectory;
      Dirty |= string.Equals (newSetting, newSetting);
    }

    private void btnResetOnlineUpdOthers_Click (object sender, EventArgs e) {
      _flagResetUpdateOthers = true;
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

      Settings.SettingsTab = tabControl1.SelectedIndex;

      updateSettingsFromControlsGeneral ();
      updateSettingsFromControlsFolder ();
      updateSettingsFromControlsConversion ();
      updateSettingsFromControlsChapters ();
      updateSettingsFromControlsMetaTags ();
    }

    private void updateSettingsFromControlsGeneral () {
      // tab page General

      Settings.RelaxedFFmpegVersionCheck = updateSettings (Settings.RelaxedFFmpegVersionCheck, ckBoxFfmpegVersCheck.Checked);
      
      bool ck = ckBoxFileAssoc.Checked;
      if ((Settings.FileAssoc ?? false) != ck) {
        Settings.FileAssoc = ck;
        new FileAssoc (Settings, this).Update ();
      }

      Settings.FileDateColumn = updateSettings (Settings.FileDateColumn, ckBoxDateClm.Checked, true);

      Settings.AaxCopyMode = updateSettings (Settings.AaxCopyMode, _cbAdapterAaxCopyMode.Value);

      Settings.AutoLaunchPlayer = ckBoxLaunchPlayer.Checked;
      
      Settings.OnlineUpdate = (EOnlineUpdate)comBoxUpdate.SelectedIndex;

      if (_flagResetUpdateOthers)
        Settings.OnlineUpdateOthersDeclined = string.Empty;

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

    private void updateSettingsFromControlsFolder () {
      // tab page Folder
      Settings.FlatFolders = updateSettings (Settings.FlatFolders, ckBoxFlatFolders.Checked);
      Settings.FlatFolderNaming = updateSettings (Settings.FlatFolderNaming, (EFlatFolderNaming)comBoxFlatFolders.SelectedIndex);

      Settings.WithSeriesTitle = updateSettings (Settings.WithSeriesTitle, ckBoxSeries.Checked);
      Settings.NumDigitsSeriesSeqNo = updateSettings (Settings.NumDigitsSeriesSeqNo, (byte)numUpDnSeriesDigits.Value);
      
      Settings.FullCaptionBookFolder = updateSettings (Settings.FullCaptionBookFolder, ckBoxFullCaptionBookFolder.Checked);

      Settings.PartNaming = updateSettings (Settings.PartNaming, (EGeneralNaming)comBoxPartName.SelectedIndex);
      Settings.PartName = updateSettings (Settings.PartName, txtBoxPartName.Text);

      Settings.OutFolderConflict =
        updateSettings (Settings.OutFolderConflict, (EOutFolderConflict)comBoxOutFolderConflict.SelectedIndex);
    }

    private void updateSettingsFromControlsConversion () {
      // tab page Conversion
      
      Settings.PartNames = updateSettings (Settings.PartNames, txtBoxCustPart.Text);
      
      Settings.AddnlValTitlePunct = updateSettings (Settings.AddnlValTitlePunct, txtBoxCustTitleChars.Text);
      
      Settings.IntermedCopySingle = updateSettings (Settings.IntermedCopySingle, ckBoxIntermedCopySingle.Checked);
      
      Settings.FixAACEncoding = updateSettings (Settings.FixAACEncoding, (EFixAACEncoding)comBoxFixAacEncoding.SelectedIndex);
      
      Settings.VariableBitRate = updateSettings (Settings.VariableBitRate, ckBoxVarBitRate.Checked);
      
      Settings.ReducedBitRate = updateSettings (Settings.ReducedBitRate, (EReducedBitRate)comBoxRedBitRate.SelectedIndex);
      
      Settings.M4B = updateSettings (Settings.M4B, comBoxM4B.SelectedIndex == 1);
      
      Settings.Latin1EncodingForPlaylist = updateSettings (Settings.Latin1EncodingForPlaylist, ckBoxLatin1.Checked);
      
      Settings.ExtraMetaFiles = updateSettings (Settings.ExtraMetaFiles, ckBoxExtraMetaFiles.Checked);
    }

    private void updateSettingsFromControlsChapters () {
      // tab page Chapters
      Settings.NamedChapters = updateSettings (Settings.NamedChapters, (ENamedChapters)comBoxNamedChapters.SelectedIndex);

      Settings.ShortChapterSec = updateSettings (Settings.ShortChapterSec, (uint)nudShortChapter.Value);
      
      Settings.VeryShortChapterSec = updateSettings (Settings.VeryShortChapterSec, (uint)nudVeryShortChapter.Value);
      
      Settings.VerifyAdjustChapterMarks = 
        updateSettings (Settings.VerifyAdjustChapterMarks, (EVerifyAdjustChapterMarks)comBoxVerAdjChapters.SelectedIndex);
      
      Settings.PreferEmbeddedChapterTimes = 
        updateSettings (Settings.PreferEmbeddedChapterTimes, (EPreferEmbeddedChapterTimes)comBoxPrefEmbChapTimes.SelectedIndex);
    }

    private void updateSettingsFromControlsMetaTags () {
      // tab page Meta Tags

      Settings.TagArtist = updateSettings (Settings.TagArtist, roleOfIndex (comBoxArtist.SelectedIndex));
      Settings.TagAlbumArtist = updateSettings (Settings.TagAlbumArtist, roleOfIndex (comBoxAlbumArtist.SelectedIndex));
      Settings.TagComposer = updateSettings (Settings.TagComposer, roleOfIndex (comBoxComposer.SelectedIndex));
      Settings.TagConductor = updateSettings (Settings.TagConductor, roleOfIndex(comBoxConductor.SelectedIndex));
    }

    private T updateSettings<T> (T oldValue, T newValue, bool affectsListView = false) {
      if (!object.Equals (oldValue, newValue)) {
        Dirty = true;
        if (affectsListView)
          ListViewDirty = true;
      }
      return newValue;
    }

    private void comBoxPartName_SelectedIndexChanged (object sender, EventArgs e) {
      if (_flag)
        return;
      var partNaming = (EGeneralNaming)comBoxPartName.SelectedIndex;
      txtBoxPartName.Enabled = partNaming == EGeneralNaming.custom;
      updatePartNaming ();
    }

    private void ckBoxFlatFolders_CheckedChanged (object sender, EventArgs e) {
      bool flatFolders = ckBoxFlatFolders.Checked;
      enableFlatFoldersDependencies (flatFolders);
    }

    private void enableFlatFoldersDependencies (bool flatFolders) {
      comBoxFlatFolders.Enabled = flatFolders;
      lblFullCaptionBookFolder.Enabled = !flatFolders;
      ckBoxFullCaptionBookFolder.Enabled = !flatFolders;
      lblSeries.Enabled = !flatFolders;
      panelSeries.Enabled = !flatFolders;
    }

    private void ckBoxSeries_CheckedChanged (object sender, EventArgs e) {
      bool withSeries = ckBoxSeries.Checked;
      panelSeriesDigits.Enabled = withSeries;
    }


    private void comBoxAaxCopy_SelectedIndexChanged (object sender, EventArgs e) {
      if (_flag || _cbAdapterAaxCopyMode is null)
        return;
      btnAaxCopyDir.Enabled = _cbAdapterAaxCopyMode.Value != default;
      if (btnAaxCopyDir.Enabled) {
        if (string.IsNullOrWhiteSpace (Settings.AaxCopyDirectory) || !Directory.Exists(Settings.AaxCopyDirectory))
          MsgBox.Show (this, R.MsgAaxCopyNoFolderYet, R.MsgAaxCopyFolder, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
    }

    private void btnAaxCopyDir_Click (object sender, EventArgs e) {
      string dir = MainForm.SetDestinationDirectory (this, Settings.AaxCopyDirectory, R.Audible, this.Text, R.MsgAaxCopyFolder, R.MsgAaxCopyNoFolder);
      if (dir is null)
        return;

      Settings.AaxCopyDirectory = dir;
    }

    private void comBoxNamedChapters_SelectedIndexChanged (object sender, EventArgs e) => 
      enablePreferEmbeddedChapterTimes ();

    private void enablePreferEmbeddedChapterTimes () {
      bool useNamedChapters = comBoxNamedChapters.SelectedIndex > 0;
      lblPrefEmbChapTimes.Enabled = useNamedChapters;
      comBoxPrefEmbChapTimes.Enabled = useNamedChapters;
    }

    private void ckBoxFfmpegVersCheck_CheckedChanged (object sender, EventArgs e) {
      bool succ = _converter.VerifyFFmpegPathVersion (_callback, ckBoxFfmpegVersCheck.Checked);
      if (!succ)
        btnFfmpegLoc_Click (sender, e);
    }

    private void tabControl1_DrawItem (object sender, DrawItemEventArgs e) {
      TabPage tp = tabControl1.TabPages[e.Index];
      using (SolidBrush brush =
             new SolidBrush (Enabled ? tp.BackColor : SystemColors.ControlLight))
      using (SolidBrush textBrush =
             new SolidBrush (Enabled ? tp.ForeColor : SystemColors.ControlDark)) {
        e.Graphics.FillRectangle (brush, e.Bounds);
        e.Graphics.DrawString (tp.Text, e.Font, textBrush, e.Bounds.X + 1, e.Bounds.Y + 3);
      }
    }

  }
}
