using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using audiamus.aaxconv.lib;
using audiamus.aux;
using audiamus.aux.win;
using audiamus.aux.win.ex;
using Microsoft.WindowsAPICodePack.Dialogs;
using static audiamus.aux.ApplEnv;

namespace audiamus.aaxconv {

  using R = Properties.Resources;

  public partial class MainForm : Form {

    #region Private Fields

    readonly AaxAudioConverter _converter;
    readonly List<AaxFileItemEx> _fileItems = new List<AaxFileItemEx> ();
    readonly ListViewColumnSorter _lvwColumnSorter;
    readonly ISettings _settings = Properties.Settings.Default;
    readonly PGANaming _pgaNaming;
    readonly ProgressProcessor _progress;
    readonly InteractionCallbackHandler<EInteractionCustomCallback> _interactionHandler;
    readonly SystemMenu _systemMenu;

    FileDetailsForm _detailsForm;
    PreviewForm _previewForm;

    CancellationTokenSource _cts;
    bool _initDone;
    bool _closing;
    bool _resetFlag;
    bool _ffMpegPathVerified;
    bool _updateAvailableFlag; 

    Point _contextMenuPoint;

    #endregion Private Fields
    #region Private Properties

    private ISettings Settings => _settings;

    #endregion Private Properties
    #region Public Constructors

    public MainForm () {
      InitializeComponent ();

      _systemMenu = new SystemMenu (this);
      _systemMenu.AddCommand (R.SysMenuItemAbout, onSysMenuAbout, true);
      _systemMenu.AddCommand (R.SysMenuItemSettings, onSysMenuBasicSettings, false);
      _systemMenu.AddCommand ($"{R.SysMenuItemHelp}\tF1", onSysMenuHelp, false);
      
      _progress = new ProgressProcessor (progressBarPart, progressBarTrack, lblProgress);

      _pgaNaming = new PGANaming (Settings) { RefreshDelegate = propGridNaming.Refresh };


      _converter = new AaxAudioConverter (Settings, Resources.Default);

      initRadionButtons ();

      _lvwColumnSorter = new ListViewColumnSorter {
        SortModifier = ListViewColumnSorter.ESortModifiers.SortByTagOrText
      };

      listViewAaxFiles.ListViewItemSorter = _lvwColumnSorter;
      listViewAaxFiles.Sorting = SortOrder.Ascending;

      _interactionHandler = new InteractionCallbackHandler<EInteractionCustomCallback> (this, customInteractionHandler);

    }

    #endregion Public Constructors
    #region Protected Methods

    protected override void OnActivated (EventArgs e) {
      base.OnActivated (e);

      if (_initDone)
        return;
      _initDone = true;
      makeFileAssoc ();

      checkAudibleActivationCode ();

      ensureFFmpegPath ();
      enableAll (true);

      var args = Environment.GetCommandLineArgs ();
      if (args.Length > 1)
        addFile (args[1]);
    }

    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);
      
      if (Settings.OutputDirectory != null && Directory.Exists (Settings.OutputDirectory))
        lblSaveTo.SetTextAsPathWithEllipsis (Settings.OutputDirectory);

      propGridNaming.SelectedObject = _pgaNaming;
      propGridNaming.Refresh ();
      
      checkOnlineUpdate ();
    }

    protected override void OnClosing (CancelEventArgs e) {
      _closing = true;
      Properties.Settings.Default.Save ();
      base.OnClosing (e);
      if (!(_cts is null)) {
        _cts.Cancel ();
        Thread.Sleep (2000); // deliberately in the main thread, to make sure ffmpeg processes receive the quit signal
      }

      if (_updateAvailableFlag) {
        e.Cancel = true;
        handleDeferredUpdateAsync ();
      }
    }

    protected override void OnKeyDown (KeyEventArgs e) {
      if (e.Modifiers == Keys.Control) {
        if (e.KeyCode == Keys.A)
          selectAll ();
        else
          base.OnKeyDown (e);
      } else {
        if (e.KeyCode == Keys.F1)
          onSysMenuHelp ();
        else
          base.OnKeyDown (e);
      }
    }

    protected override void WndProc (ref Message m) {
      base.WndProc (ref m);
      _systemMenu.HandleMessage (ref m);
    }


    #endregion Protected Methods

    #region Private Methods

    private async void checkOnlineUpdate () {
      var update = new OnlineUpdate (Settings, Resources.Default);
      await update.UpdateAsync (_interactionHandler, () => Application.Exit(), isBusyForUpdate);
    }

    private async void handleDeferredUpdateAsync () {
      var update = new OnlineUpdate (Settings, Resources.Default);
      await update.InstallAsync (_interactionHandler, () => Application.Exit ());
    }
    
    private bool isBusyForUpdate () {
      bool busy = this.listViewAaxFiles.Items.Count > 0;
      if (busy)
        _updateAvailableFlag = true;
      return busy;
    }

    private void reinitControlsFromSettings () {
      initRadionButtons ();
      _pgaNaming.Update ();
      lblSaveTo.SetTextAsPathWithEllipsis (Settings.OutputDirectory ?? string.Empty);
      _previewForm?.UpdatePreview ();
    }


    private void onSysMenuAbout () => new AboutForm () { Owner = this }.ShowDialog ();

    private void onSysMenuBasicSettings () {
      var dlg = new SettingsForm (_converter, _interactionHandler.Interact) { Owner = this };
      dlg.ShowDialog ();
      if (dlg.SettingsReset)
        reinitControlsFromSettings ();
      ensureFFmpegPath ();
      enableAll (true);
    }

    private void onSysMenuHelp () {
      const string PDF = ".pdf";

      string uiCultureName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
      string neutralDocFile = ApplName + PDF;

      var sb = new StringBuilder ();
      sb.Append (ApplName);
      if (uiCultureName != NeutralCultureName)
        sb.Append ($".{uiCultureName}");
      sb.Append (PDF);
      string localizedDocFile = sb.ToString();
      
      var filename = Path.Combine (ApplDirectory, localizedDocFile);
      if (!File.Exists (filename))
        filename = Path.Combine (ApplDirectory, neutralDocFile);

      try {
        Process.Start (filename);
      } catch (Exception) {
      }

    }


    private void checkAudibleActivationCode () {
      if (_converter.HasActivationCode)
        return;

      new ActivationCodeForm () { Owner = this }.ShowDialog ();

    }


    private void makeFileAssoc () {
      if (Settings.FileAssoc.HasValue && !Settings.FileAssoc.Value)
        return;

      new FileAssoc (Settings, this).AssociateInit ();

    }


    private void selectAll () {
      foreach (ListViewItem lvi in listViewAaxFiles.Items)
        lvi.Selected = true;
    }

    private async void addFile (string file) {
      if (!File.Exists (file))
        return;
      await addFilesAsync (new[] { file });
    }


    private async Task addFilesAsync (string[] filenames) {
      enableAll (false);
      this.UseWaitCursor = true;

      var addedFileItems = await _converter.AddFilesAsync (filenames);

      if (addedFileItems.Count () > 0) {

        foreach (var fi in addedFileItems)
          _fileItems.Add (new AaxFileItemEx (fi));

        var fileItems = _fileItems.Distinct ().OrderBy (x => x.FileItem.BookTitle).ToList ();
        _fileItems.Clear ();
        _fileItems.AddRange (fileItems);

        refillListView ();
      }

      this.UseWaitCursor = false;

      enableAll (true);
      listViewAaxFiles.Select ();
      enableButtonConvert ();

    }

    private void enableAll (bool enable) {
      enable &= _converter.HasActivationCode && _ffMpegPathVerified;
      panelTop.Enabled = enable;
      panelExec.Enabled = enable;
    }

    private void ensureFFmpegPath () {
      bool succ = false;
      if (!string.IsNullOrWhiteSpace (Settings.FFMpegDirectory)) {
        succ = _converter.VerifyFFmpegPathVersion (_interactionHandler.Interact);
      } else {
        string ffmpegdir = ApplEnv.ApplDirectory;
        string path = Path.Combine (ffmpegdir, FFmpeg.FFMPEG_EXE);
        if (File.Exists (path)) {
          Settings.FFMpegDirectory = ffmpegdir;
          using (new ResourceGuard (() => Settings.FFMpegDirectory = null))
            succ = _converter.VerifyFFmpegPathVersion (_interactionHandler.Interact);
        }
      }

      if (!succ) {

        FFmpegLocationForm dlg = new FFmpegLocationForm (_converter, _interactionHandler.Interact);
        var result = dlg.ShowDialog ();
        succ = result == DialogResult.OK;
      }

      _ffMpegPathVerified = succ;
    }


    private void initRadionButtons () {
      using (new ResourceGuard (f => _resetFlag = f)) {
        radBtnM4a.Checked = Settings.ConvFormat == EConvFormat.m4a;
        radBtnMp3.Checked = Settings.ConvFormat == EConvFormat.mp3;

        radBtnSingle.Checked = false;
        radBtnChapt.Checked = false;
        radBtnChaptSplit.Checked = false;
        radBtnTimeSplit.Checked = false;

        switch (Settings.ConvMode) {
          case EConvMode.single:
            radBtnSingle.Checked = true;
            break;
          case EConvMode.chapters:
            radBtnChapt.Checked = true;
            break;
          case EConvMode.splitChapters:
            radBtnChaptSplit.Checked = true;
            break;
          case EConvMode.splitTime:
            radBtnTimeSplit.Checked = true;
            break;
        }

        numUpDnTrkLen.Value = Settings.TrkDurMins;
        panelTrkLen.Enabled = Settings.ConvMode >= EConvMode.splitChapters;
      }
    }

    private void refillListView () {
      listViewAaxFiles.Items.Clear ();
      foreach (var pr in _fileItems) {
        var fi = pr.FileItem;
        var lvi = pr.ListViewItem;
        if (lvi is null) {
          lvi = new ListViewItem {
            Text = fi.BookTitle
          };
          lvi.SubItems.Add (fi.Author);
          lvi.SubItems.Add ($"{fi.FileSize / (1024 * 1024)} MB");
          lvi.SubItems.Add (fi.Duration.ToString (@"hh\:mm\:ss"));
          lvi.SubItems.Add (fi.PublishingDate?.Year.ToString ());
          lvi.SubItems.Add (fi.Narrator);
          lvi.SubItems.Add ($"{fi.SampleRate} Hz");
          lvi.SubItems.Add ($"{fi.AvgBitRate} kb/s");

          pr.ListViewItem = lvi;
        }
        listViewAaxFiles.Items.Add (lvi);
      }

      if (listViewAaxFiles.Items.Count > 0) {
        listViewAaxFiles.AutoResizeColumns (ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewAaxFiles.AutoResizeColumns (ColumnHeaderAutoResizeStyle.HeaderSize);
      }

      if (listViewAaxFiles.Items.Count == 1)
        listViewAaxFiles.Items[0].Selected = true;
    }

    private bool? customInteractionHandler (InteractionMessage<EInteractionCustomCallback> im) {
      switch (im.Custom) {
        default:
          return null;
        case EInteractionCustomCallback.noActivationCode:
          return new ActivationCodeForm(true).ShowDialog() == DialogResult.OK;
      }
    }

    private void enableButtonConvert () {
      btnConvert.Enabled = listViewAaxFiles.SelectedIndices.Count > 0 && _converter.HasActivationCode;
      if (btnConvert.Enabled)
        this.AcceptButton = btnConvert;
      else
        this.AcceptButton = null;

    }

    private void radioButton<T> (object sender, Action<T> action, T value) where T : struct, Enum {
      if (_resetFlag)
        return;
      if (sender is RadioButton rb && rb.Checked) {
        action (value);
        panelTrkLen.Enabled = Settings.ConvMode >= EConvMode.splitChapters;
        enableButtonConvert ();
      }
    }


    private void openFileDetailsForm (AaxFileItem fi) => openNonModalForm (fi, ref _detailsForm, detailsForm_FormClosed);

    private void openPreviewForm (AaxFileItem fi) => openNonModalForm (fi, ref _previewForm, previewForm_FormClosed);

    private void openNonModalForm<F> (AaxFileItem fi, ref F form, FormClosedEventHandler handler) where F: FileItemForm, new() {
      if (form is null) {
        form = new F { Owner = this, Location = _contextMenuPoint, Previewer = _converter };
        form.FormClosed += handler;
        form.Set (fi);
        form.Show ();
      } else {
        //form.Location = _contextMenuPoint;
        form.Set (fi);
      }
    }

    private AaxFileItem getFocusedFileItem () {
      var lvi = listViewAaxFiles.FocusedItem;
      var fi = _fileItems.Where (i => i.ListViewItem == lvi).Select (i => i.FileItem).FirstOrDefault ();
      return fi;
    }


    #endregion Private Methods

    #region Event Handlers

    private void detailsForm_FormClosed (object sender, FormClosedEventArgs e) => _detailsForm = null;

    private void previewForm_FormClosed (object sender, FormClosedEventArgs e) => _previewForm = null;

    private async void btnAddFile_Click (object sender, EventArgs e) {
      OpenFileDialog dlg = new OpenFileDialog {
        InitialDirectory = Settings.InputDirectory,
        Multiselect = true,
        CheckFileExists = true,
        Filter = R.FilterAudibleFiles,
      };

      var result = dlg.ShowDialog ();
      if (result != DialogResult.OK)
        return;

      var filenames = dlg.FileNames;
      await addFilesAsync (filenames);

    }

    private void btnRem_Click (object sender, EventArgs e) {
      var result = MsgBox.Show (this, R.MsgRemSelFiles, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result != DialogResult.Yes)
        return;

      var selItems = listViewAaxFiles.SelectedItems;

      var fileItems = _fileItems.Where (x => selItems.Contains (x.ListViewItem))
        .OrderByDescending (x => x.FileItem.FileName).ToList ();

      foreach (var fi in fileItems)
        _fileItems.Remove (fi);

      refillListView ();

      btnRem.Enabled = false;
      enableButtonConvert ();

    }

    private void listViewAaxFiles_ColumnClick (object sender, ColumnClickEventArgs e) {
      ListView lv = (ListView)sender;

      // Determine if clicked column is already the column that is being sorted.
      if (e.Column == _lvwColumnSorter.SortColumn) {
        // Reverse the current sort direction for this column.
        if (_lvwColumnSorter.Order == SortOrder.Ascending) {
          _lvwColumnSorter.Order = SortOrder.Descending;
        } else {
          _lvwColumnSorter.Order = SortOrder.Ascending;
        }
      } else {
        // Set the column number that is to be sorted; default to ascending.
        _lvwColumnSorter.SortColumn = e.Column;
        _lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      lv.Sort ();

    }

    private async void listViewAaxFiles_DragDrop (object sender, DragEventArgs e) {
      this.Activate ();
      string[] s = (string[])e.Data.GetData (DataFormats.FileDrop, false);
      await addFilesAsync (s);
    }

    private void listViewAaxFiles_DragEnter (object sender, DragEventArgs e) {
      if (e.Data.GetDataPresent (DataFormats.FileDrop))
        e.Effect = DragDropEffects.All;
      else
        e.Effect = DragDropEffects.None;
    }

    private void listViewAaxFiles_KeyDown (object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.Delete)
        btnRem_Click (sender, null);
    }

    private void listViewAaxFiles_SelectedIndexChanged (object sender, EventArgs e) {
      btnRem.Enabled = listViewAaxFiles.SelectedIndices.Count > 0;
      enableButtonConvert ();
    }


    private void listViewAaxFiles_MouseClick (object sender, MouseEventArgs e) {
      if (e.Button == MouseButtons.Right) {
        if (listViewAaxFiles.FocusedItem.Bounds.Contains (e.Location)) {
          _contextMenuPoint = Cursor.Position;
          contextMenuStrip1.Show (_contextMenuPoint);
        }
      }
    }

    private void tsmiDetails_Click (object sender, EventArgs e) {
      AaxFileItem fi = getFocusedFileItem ();
      if (fi is null)
        return;

      openFileDetailsForm (fi);
    }


    private void tsmiPreview_Click (object sender, EventArgs e) {
      AaxFileItem fi = getFocusedFileItem ();
      if (fi is null)
        return;

      openPreviewForm (fi);
    }


    private void propGridNaming_PropertyValueChanged (object s, PropertyValueChangedEventArgs e) {
      _previewForm?.UpdatePreview ();
      enableButtonConvert ();
    }

    private void radBtnM4A_CheckedChanged (object sender, EventArgs e) => 
      radioButton (sender, v => Settings.ConvFormat = v, EConvFormat.m4a);

    private void radBtnMp3_CheckedChanged (object sender, EventArgs e) => 
      radioButton (sender, v => Settings.ConvFormat = v, EConvFormat.mp3);

    private void radBtnSingle_CheckedChanged (object sender, EventArgs e) => 
      radioButton (sender, v => Settings.ConvMode = v, EConvMode.single);

    private void radBtnChapt_CheckedChanged (object sender, EventArgs e) => 
      radioButton (sender, v => Settings.ConvMode = v, EConvMode.chapters);

    private void radBtnChaptSplit_CheckedChanged (object sender, EventArgs e) => 
      radioButton (sender, v => Settings.ConvMode = v, EConvMode.splitChapters);

    private void radBtnTimeSplit_CheckedChanged (object sender, EventArgs e) =>
      radioButton (sender, v => Settings.ConvMode = v, EConvMode.splitTime);

    private async void btnConvert_Click (object sender, EventArgs e) {

      if (!Directory.Exists (Settings.OutputDirectory))
        btnSaveTo_Click (sender, e);
      if (!Directory.Exists (Settings.OutputDirectory))
        return;

      btnAbort.Enabled = true;
      btnConvert.Enabled = false;
      this.AcceptButton = btnAbort;

      panelTop.Enabled = false;


      var progress = new Progress<ProgressMessage> (_progress.Update);
      var callback = new InteractionCallback<InteractionMessage, bool?> (_interactionHandler.Interact);
      foreach (var lvi in listViewAaxFiles.SelectedItems) {
        (lvi as ListViewItem).ImageIndex = 0;
      }

      var fileitems = _fileItems
        .Where (x => listViewAaxFiles.SelectedItems
        .Contains (x.ListViewItem))
        .Select (x => x.FileItem)
        .OrderBy (x => x.BookTitle)
        .ToList ();

      var cursor = this.Cursor;
      this.Cursor = Cursors.AppStarting;

      _progress.Reset ();
      _cts = new CancellationTokenSource ();

      try {
        await _converter.ConvertAsync (fileitems, _cts.Token, progress, callback);
      } catch (OperationCanceledException) { }

      if (_closing)
        return;

      _cts.Dispose ();
      _cts = null;

      this.Cursor = cursor;

      var converted = fileitems.Where (x => x.Converted);
      var lvis = _fileItems.Where (x => converted.Contains (x.FileItem)).Select (x => x.ListViewItem);
      foreach (var lvi in lvis)
        lvi.ImageIndex = 1;

      this.AcceptButton = null;
      panelTop.Enabled = true;
      btnAbort.Enabled = false;
      listViewAaxFiles.Select ();

      int cnt = converted.Count ();
      MessageBoxIcon mbIcon;
      string intro;
      if (cnt == 0) {
        intro = R.MsgAborted;
        mbIcon = MessageBoxIcon.Stop;
      } else if (cnt != fileitems.Count) {
        intro = R.MsgSomeDone;
        mbIcon = MessageBoxIcon.Warning;
      } else { 
        intro = R.MsgAllDone;
        mbIcon = MessageBoxIcon.Information;
      } 

      MsgBox.Show (this, $"{intro}. {converted.Count()} {(cnt == 1 ? R.file : R.files)} {R.MsgConverted}.", this.Text, 
        MessageBoxButtons.OK, mbIcon);

      _progress.Reset ();

    }

    private void btnAbort_Click (object sender, EventArgs e) {
      _cts?.Cancel ();
      btnAbort.Enabled = false;
    }


    private void btnSaveTo_Click (object sender, EventArgs e) {
      string defdir = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
      string dir = Settings.OutputDirectory;
      if (string.IsNullOrWhiteSpace (dir) || !Directory.Exists(dir))
        dir = defdir;

      CommonOpenFileDialog ofd = new CommonOpenFileDialog {
        Title = $"{this.Text}: {R.MsgCommonRootFolder}",
        InitialDirectory = dir,
        DefaultDirectory = defdir,
        IsFolderPicker = true,
        EnsurePathExists = true,
      };
      CommonFileDialogResult result = ofd.ShowDialog ();
      if (result == CommonFileDialogResult.Cancel)
        return;

      Settings.OutputDirectory = ofd.FileName;
      lblSaveTo.SetTextAsPathWithEllipsis (ofd.FileName);

      enableButtonConvert ();

    }

    private void numUpDnTrkLen_ValueChanged (object sender, EventArgs e) {
      Settings.TrkDurMins = (byte)numUpDnTrkLen.Value;
      enableButtonConvert ();
    }
    #endregion Event Handlers
  }
}
