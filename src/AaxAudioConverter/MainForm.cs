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
using audiamus.aaxconv.lib.ex;
using audiamus.aux;
using audiamus.aux.ex;
using audiamus.aux.win;
using audiamus.aux.win.ex;
using audiamus.perf;
using Microsoft.WindowsAPICodePack.Dialogs;
using static audiamus.aux.ApplEnv;
using static audiamus.aux.Logging;
using Timer = System.Windows.Forms.Timer;

namespace audiamus.aaxconv {

  using R = Properties.Resources;

  public partial class MainForm : Form {

    #region Private Fields

    private readonly AaxAudioConverter _converter;
    private readonly List<AaxFileItemEx> _fileItems = new List<AaxFileItemEx> ();
    private readonly ListViewColumnSorter _lvwColumnSorter;
    private readonly IAppSettings _settings = Properties.Settings.Default;
    private readonly PGANaming _pgaNaming;
    private readonly ProgressProcessor _progress;
    private readonly InteractionCallbackHandler<EInteractionCustomCallback> _interactionHandler;
    private readonly SystemMenu _systemMenu;
    private readonly Timer _resizeTimer = new Timer ();

    private readonly PerformanceMonitor _perfMonitor;
    private readonly PerformanceHandler _perfHandler;
    private readonly IProgress<IPerfCallback> _perfProgress; 

    private FileDetailsForm _detailsForm;
    private PreviewForm _previewForm;

    private CancellationTokenSource _cts;
    private bool _initDone;
    private bool _closing;
    private bool _resetFlagRadioButtons;
    private bool _resetFlagTrackLength;
    private bool _ffMpegPathVerified;
    private bool _updateAvailableFlag; 
    private bool _resizeFlag; 

    private Point _contextMenuPoint;

    #endregion Private Fields
    #region Private Properties

    private IAppSettings Settings => _settings;
    private string DefaultInputDirectory => defaultInputDirectory ();

    #endregion Private Properties
    #region Public Constructors

    public MainForm () {
      using (new ResourceGuard (x => _resizeFlag = x))
        InitializeComponent ();

      Log (1, this, () => $"{ApplName} {AssemblyVersion} as {(Is64BitProcess ? "64" : "32")}bit process on Windows {OSVersion} {(Is64BitOperatingSystem ? "64" : "32")}bit");

      _systemMenu = new SystemMenu (this);
      _systemMenu.AddCommand (R.SysMenuItemAbout, onSysMenuAbout, true);
      _systemMenu.AddCommand (R.SysMenuItemSettings, onSysMenuBasicSettings, false);
      _systemMenu.AddCommand ($"{R.SysMenuItemHelp}\tF1", onSysMenuHelp, false);

      _progress = new ProgressProcessor (progressBarPart, progressBarTrack, lblProgress);

      _pgaNaming = new PGANaming (Settings) {
        RefreshDelegate = propGridNaming.Refresh,
        IsInSplitChapterMode = () => Settings.ConvMode == EConvMode.splitChapters
      };

      presetListView ();

      Settings.FixNarrator ();

      _converter = new AaxAudioConverter (Settings, Resources.Default);

      initRadionButtons ();

      _lvwColumnSorter = new ListViewColumnSorter {
        SortModifier = ListViewColumnSorter.ESortModifiers.SortByTagOrText
      };

      listViewAaxFiles.ListViewItemSorter = _lvwColumnSorter;
      listViewAaxFiles.Sorting = SortOrder.Ascending;
      _lvwColumnSorter.Order = SortOrder.Ascending;

      _interactionHandler = new InteractionCallbackHandler<EInteractionCustomCallback> (this, customInteractionHandler);

      _perfHandler = new PerformanceHandler (vprogbarNumProc, vprogbarCpu, toolTip1);
      _perfProgress = new Progress<IPerfCallback> (_perfHandler.Update);
      _perfMonitor = new PerformanceMonitor { Callback = _perfProgress.Report };

      _resizeTimer.Tick += resizeTimer_Tick;
      _resizeTimer.Interval = 100;
    }

    #endregion Public Constructors
    #region Internal Methods

    internal static string SetDestinationDirectory (
      IWin32Window owner, string directory, string defsubdir,
      string windowtitle, string caption, string cancelMessage = null
    ) {
      string defdir = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
      if (!Directory.Exists (defdir))
        defdir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);

      defdir = Path.Combine (defdir, defsubdir);
      string dir = directory;
      if (string.IsNullOrWhiteSpace (dir) || !Directory.Exists (dir))
        dir = defdir;

      bool noDefDir = false;
      try {
        if (dir == defdir && !Directory.Exists (dir))
          Directory.CreateDirectory (defdir);
      } catch (Exception exc) {
        noDefDir = true;
        Log (1, typeof (MainForm), $"\"{defdir.SubstitUser ()}\": {exc.ToShortString ()}");
      }

      CommonOpenFileDialog ofd = new CommonOpenFileDialog {
        Title = $"{windowtitle}: {caption}",
        InitialDirectory = dir,
        IsFolderPicker = true,
        EnsurePathExists = true,
      };
      if (!noDefDir)
        ofd.DefaultDirectory = defdir;

      CommonFileDialogResult result = ofd.ShowDialog ();
      if (result == CommonFileDialogResult.Cancel) {
        if (!string.IsNullOrWhiteSpace (cancelMessage) && !Directory.Exists (directory))
          MsgBox.Show (owner, cancelMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return null;
      }

      return ofd.FileName;

    }

    #endregion Internal Methods

    #region Protected Methods

    protected override async void OnActivated (EventArgs e) {
      base.OnActivated (e);

      if (_initDone)
        return;
      _initDone = true;

      _pgaNaming.Update ();

      showWhatsNew ();

      makeFileAssoc ();

      checkAudibleActivationCode ();

      showStartupHint ();

      enableAll (true);
      var args = Environment.GetCommandLineArgs ();
      if (args.Length > 1)
        addFile (args[1]);

      await ensureFFmpegPathAsync ();
    }

    protected override void OnLoad (EventArgs e) {
      base.OnLoad (e);

      presetInpuDirectory ();

      setLabelSaveTo ();

      propGridNaming.SelectedObject = _pgaNaming;
      propGridNaming.Refresh ();

      checkOnlineUpdate ();
    }


    private void setLabelSaveTo () {
      if (Settings.OutputDirectory != null && Directory.Exists (Settings.OutputDirectory))
        lblSaveTo.SetTextAsPathWithEllipsis (Settings.OutputDirectory);
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
        if (e.KeyCode == Keys.A) {
          e.SuppressKeyPress = true;
          selectAll ();
        } else
          base.OnKeyDown (e);
      } else {
        if (e.KeyCode == Keys.F1) {
          e.SuppressKeyPress = true;
          onSysMenuHelp ();
        } else
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

    private string defaultInputDirectory () {
      string defdir = Environment.GetFolderPath (Environment.SpecialFolder.MyMusic);
      string defsubdir = Path.Combine (defdir, R.Audible);
      if (Directory.Exists (defsubdir))
        return defsubdir;
      else
        return defdir;
    }

    private void presetInpuDirectory () {
      if (!string.IsNullOrEmpty(Settings.InputDirectory) && Directory.Exists (Settings.InputDirectory))
        return;

      string inputDir = _converter.AppContentDirectory;
      if (inputDir is null)
        inputDir = DefaultInputDirectory; 

      Settings.InputDirectory = inputDir;
    }

    private void reinitControlsFromSettings () {
      initRadionButtons ();
      presetListView (true);
      _pgaNaming.Update ();
      lblSaveTo.SetTextAsPathWithEllipsis (Settings.OutputDirectory ?? string.Empty);
      _previewForm?.UpdatePreview ();
    }


    private void onSysMenuAbout () => new AboutForm () { Owner = this }.ShowDialog ();

    private void onSysMenuBasicSettings () {
      var dlg = new SettingsForm (_converter, _interactionHandler.Interact) {
        Owner = this,
        Enabled = !btnAbort.Enabled
      };
      dlg.ShowDialog ();
      if (dlg.SettingsReset)
        reinitControlsFromSettings ();
      else
        initRadionButtons ();

      if (dlg.Dirty) {
        enableButtonConvert ();
        if (dlg.ListViewDirty) {
          presetListView (true);
          btnConvert.Enabled = false;
          listViewAaxFiles.Focus ();
        }
        _ffMpegPathVerified = _converter.VerifiedFFmpegPath;
      }
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

      var result = new ActivationCodeForm () { Owner = this }.ShowDialog ();
      if (result == DialogResult.OK)
        _converter.ReinitActivationCode ();
    }


    private void makeFileAssoc () {
      if (Settings.FileAssoc.HasValue && !Settings.FileAssoc.Value)
        return;

      new FileAssoc (Settings, this).AssociateInit ();
    }

    private void showWhatsNew () {
      // Window should only be shown to existing users after update.
      // Version should be saved to settings for new users as well.

      // new users: Settings.FileAssoc is null -and- Settings.Version is null or white space.
      bool newuser = Settings.FileAssoc.IsNull () && Settings.Version.IsNullOrWhiteSpace ();

      // update: settings version number is null or lower than assembly  
      Version.TryParse (Settings.Version, out var version);
      bool update = version is null || version < AssemblyVersion;

      bool show = update && !newuser;
      if (show) {
        var dlg = new WhatsNewForm { Owner = this };
        dlg.ShowDialog ();
      }
      
      if (update) {
        Settings.Version = AssemblyVersion.ToString ();
        Settings.Save ();
      }
    }

    private void showStartupHint () {
      if (Settings.ShowStartupTip.HasValue && !Settings.ShowStartupTip.Value)
        return;

      var dlg = new StartupTipForm (Settings) { Owner = this };
      dlg.Show ();
    }


    private void selectAll () {
      foreach (ListViewItem lvi in listViewAaxFiles.Items)
        lvi.Selected = true;
    }

    private async void addFile (string file) {
      if (file.StartsWith ("-") && file.Contains ("=")) // a cmd line arg
        return;

      Log (3, this, () => $"\"{file.SubstitUser ()}\"");
      file = file.AsUncIfLong ();
      if (!File.Exists (file)) {
        Log (3, this, $"fail: \"{file.SubstitUser ()}\"");
        return;
      }
      await addFilesAsync (new[] { file });
    }


    private async Task addFilesAsync (IEnumerable<string> filenames) {
      enableAll (false);
      this.UseWaitCursor = true;

      var callback = new InteractionCallback<InteractionMessage, bool?> (_interactionHandler.Interact);

      var addedFileItems = await _converter.AddFilesAsync (filenames, callback);

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

    private async Task ensureFFmpegPathAsync () {
      bool succ = false;
      if (!string.IsNullOrWhiteSpace (Settings.FFMpegDirectory)) {
        succ = await _converter.VerifyFFmpegPathVersionAsync (_interactionHandler.Interact);
      } else {
        string ffmpegdir = ApplEnv.ApplDirectory;
        string path = Path.Combine (ffmpegdir, FFmpeg.FFMPEG_EXE);
        if (File.Exists (path)) {
          Settings.FFMpegDirectory = ffmpegdir;
          using (new ResourceGuard (() => Settings.FFMpegDirectory = null))
            succ = await _converter.VerifyFFmpegPathVersionAsync (_interactionHandler.Interact);
        }
      }

      if (!succ) {

        FFmpegLocationForm dlg = new FFmpegLocationForm (_converter, _interactionHandler.Interact);
        var result = dlg.ShowDialog ();
        succ = result == DialogResult.OK;
      }

      _ffMpegPathVerified = succ;
      enableAll (true);
    }


    private void initRadionButtons () {
      using (new ResourceGuard (f => _resetFlagRadioButtons = f)) {

        radBtnMp4.Text = Settings.M4B ? "M4B" : "M4A";

        radBtnMp4.Checked = Settings.ConvFormat == EConvFormat.mp4;
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

        setTrackLengthControl ();

      }
    }

    private void setTrackLengthControl () {
      using (new ResourceGuard (f => _resetFlagTrackLength = f)) {
        var (min, max, val) = Settings.TrkDurMins.TrackDuration (Settings.ConvMode);
        numUpDnTrkLen.Minimum = min;
        numUpDnTrkLen.Maximum = max;
        numUpDnTrkLen.Value = val;
        panelTrkLen.Enabled = Settings.ConvMode >= EConvMode.splitChapters;
      }
    }

    private void presetListView (bool refill = false) {
      if (Settings.FileDateColumn) {
        if (!listViewAaxFiles.Columns.Contains (clmHdrFileDate))
          listViewAaxFiles.Columns.Add (clmHdrFileDate);
      } else {
        if (listViewAaxFiles.Columns.Contains (clmHdrFileDate))
          listViewAaxFiles.Columns.Remove (clmHdrFileDate);
      }

      if (refill)
        refillListView (true);
      else {
        adaptListViewColumnsWidthsForDate ();
      }
    }

    private void adaptListViewColumnsWidthsForDate () {
      if (Settings.FileDateColumn) {
        int wid = clmHdrFileDate.Width;
        clmHdrAlbum.Width -= wid / 2;
        clmHdrArtist.Width -= wid / 4;
        clmHdrNarrator.Width -= wid / 4;
      }
    }

    private void refillListView (bool recreate = false) {

      var ci = Thread.CurrentThread.CurrentCulture;
      var ciui = Thread.CurrentThread.CurrentUICulture;
      if (ciui is null)
        ciui = ci;

      Thread.CurrentThread.CurrentCulture = ciui;
      using (new ResourceGuard (() => Thread.CurrentThread.CurrentCulture = ci)) {

        listViewAaxFiles.Items.Clear ();
        foreach (var pr in _fileItems) {
          var fi = pr.FileItem;
          var lvi = pr.ListViewItem;
          if (lvi is null || recreate) {
            lvi = new ListViewItem {
              Text = fi.BookTitle,
            };
            lvi.SubItems.Add (fi.Author);
            lvi.SubItems.Add ($"{fi.FileSize / (1024 * 1024)} MB");
            lvi.SubItems.Add (fi.Duration.ToStringHMS ());
            lvi.SubItems.Add (fi.PublishingDate?.Year.ToString ());
            lvi.SubItems.Add (fi.Narrator);
            lvi.SubItems.Add ($"{fi.SampleRate} Hz");
            lvi.SubItems.Add ($"{fi.AvgBitRate} kb/s");

            if (Settings.FileDateColumn)
              lvi.SubItems.Add (fi.FileDate.ToShortDateString ());

            lvi.SubItems[0].Tag = _converter.SortingTitleWithPart (fi);
            lvi.SubItems[2].Tag = fi.FileSize;
            lvi.SubItems[6].Tag = fi.SampleRate;
            lvi.SubItems[7].Tag = fi.AvgBitRate;

            if (Settings.FileDateColumn)
              lvi.SubItems[8].Tag = fi.FileDate;

            pr.ListViewItem = lvi;

            Log (3, this, () => {
              const string CC = "taslynmbd";
              var sb = new StringBuilder ();
              int cnt = lvi.SubItems.Count;
              int n = Math.Min (CC.Length, cnt);
              for (int i = 0; i < n; i++) {
                if (sb.Length > 0)
                  sb.Append (", ");
                string qt = (i <= 1 || i == 5) ? "\"" : string.Empty;
                sb.Append ($"{CC[i]}={qt}{lvi.SubItems[i].Text}{qt}");
              }
              return sb.ToString ();
            });
          }
          listViewAaxFiles.Items.Add (lvi);
        }
      }

      adjustListViewColumnWidths ();

      if (listViewAaxFiles.Items.Count == 1)
        listViewAaxFiles.Items[0].Selected = true;

    }

    private void adjustListViewColumnWidths () {
      using (new ResourceGuard (x => _resizeFlag = x)) {
        if (listViewAaxFiles.Items.Count > 0) {
          listViewAaxFiles.AutoResizeColumns (ColumnHeaderAutoResizeStyle.ColumnContent);
          listViewAaxFiles.AutoResizeColumns (ColumnHeaderAutoResizeStyle.HeaderSize);
        } else
          adjustEmptyListViewColumnWidths ();
      }
    }

    private void adjustEmptyListViewColumnWidths () {
      const int widLast = 60;
      const int marg = 2;
      listViewAaxFiles.AutoResizeColumns (ColumnHeaderAutoResizeStyle.HeaderSize);

      int wid = 0;
      for (int i = 0; i < listViewAaxFiles.Columns.Count - 1; i++) {
        var clm = listViewAaxFiles.Columns[i];
        wid += clm.Width;
      }
      wid += widLast;
      int delta = listViewAaxFiles.Width - wid - 2 * marg; // margin

      if (delta > 0) {
        clmHdrAlbum.Width += (int)(delta * 0.45);
        clmHdrArtist.Width += delta / 4;
        clmHdrNarrator.Width += delta / 4;
        clmHdrSize.Width += delta / 20;
        listViewAaxFiles.Columns[listViewAaxFiles.Columns.Count - 1].Width = widLast + marg;
      }
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
      if (_resetFlagRadioButtons)
        return;
      if (sender is RadioButton rb && rb.Checked) {
        action (value);
        setTrackLengthControl ();
        _pgaNaming.Update ();
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
      CommonOpenFileDialog ofd = new CommonOpenFileDialog {
        Title = $"{this.Text}: {R.OpenFile}",
        DefaultDirectory = DefaultInputDirectory,
        InitialDirectory = Settings.InputDirectory,
        Multiselect = true,
        EnsurePathExists = true,
        EnsureFileExists = true,
      };
      string[] filters = R.FilterAudibleFiles?.Split ('|');
      if (!(filters is null) && filters.Length % 2 == 0) {
        for (int i = 0; i < filters.Length; i += 2) {
          string filter = filters[i];
          int pos = filter.IndexOf ('(');
          if (pos >= 0)
            filter = filter.Substring (0, pos);
          var cfdf = new CommonFileDialogFilter (filter.Trim (), filters[i + 1].Trim ());
          ofd.Filters.Add (cfdf);
        }
      }
      CommonFileDialogResult result = ofd.ShowDialog ();
      if (result != CommonFileDialogResult.Ok)
        return;

      var filenames = ofd.FileNames;

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

      Settings.ConvertByFileDate = 
        e.Column == listViewAaxFiles.Columns.IndexOf(clmHdrFileDate);

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
      radioButton (sender, v => Settings.ConvFormat = v, EConvFormat.mp4);

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

      if (Settings.AaxCopyMode != default && !Directory.Exists (Settings.AaxCopyDirectory))
        setAaxCopyDirectory ();

      btnAbort.Enabled = true;
      btnConvert.Enabled = false;
      this.AcceptButton = btnAbort;

      panelTop.Enabled = false;

      _perfMonitor.Start ();

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

      Log (1, this, () => $"#files={fileitems.Count}");

      try {
        await _converter.ConvertAsync (fileitems, _cts.Token, progress, callback);
      } catch (OperationCanceledException) { }

      if (_closing)
        return;

      _cts.Dispose ();
      _cts = null;

      _perfMonitor.Stop ();
      _perfHandler.Reset ();

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
      bool fullSuccess = cnt == fileitems.Count;

      Log (1, this, () => $"#files={fileitems.Count}, #conv={cnt}");

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

      string msg = $"{intro}. {converted.Count ()} {(cnt == 1 ? R.file : R.files)} {R.MsgConverted} in {_converter.StopwatchElapsed.ToStringHMS ()}";

      msg = addFileErrors (msg);

      MsgBox.Show (this, msg, this.Text,
        MessageBoxButtons.OK, mbIcon);

      _progress.Reset ();
      _perfHandler.Reset ();

      if (fullSuccess)
        autoplay ();
    }

    private string addFileErrors (string msg) {
      IEnumerable<string> fileerrors = _converter.FileErrors;
      int nErr = fileerrors.Count ();
      if (nErr > 0) {
        var sb = new StringBuilder (msg);
        int nMsg = 8;
        if (nErr == nMsg + 1)
          nMsg++;
        sb.AppendLine ();
        sb.AppendLine ();
        sb.AppendLine (R.MsgFFmpegFileError1);

        int i = 0;
        foreach (string fn in fileerrors) {
          sb.AppendLine ($"\"{fn}\"");
          i++;
          if (i >= nMsg)
            break;
        }
        if (nMsg < nErr) {
          int d = nErr - nMsg;
          sb.AppendLine ($"{R.MsgFFmpegFileError2} {d} {R.MsgFFmpegFileError3}");
        }
        msg = sb.ToString ();
      }

      return msg;
    }

    private void autoplay () {
      if (!Settings.AutoLaunchPlayer || _converter.AutoLaunchAudioFile is null || !File.Exists(_converter.AutoLaunchAudioFile))
        return;

      try {
        Log (3, this, $"\"{_converter.AutoLaunchAudioFile.SubstitUser()}\"");
        Process.Start (_converter.AutoLaunchAudioFile);
      } catch (Exception exc) {
        Log (1, this, $"{exc.ToShortString()}{Environment.NewLine}  \"{_converter.AutoLaunchAudioFile.SubstitUser()}\"");
      }
    }

    private void btnAbort_Click (object sender, EventArgs e) {
      Log0 (3, this);
      _cts?.Cancel ();
      btnAbort.Enabled = false;
    }


    private void btnSaveTo_Click (object sender, EventArgs e) {
      string dir = SetDestinationDirectory (this, Settings.OutputDirectory, R.Audiobook, this.Text, R.MsgCommonRootFolder);
      if (dir is null)
        return;

      Settings.OutputDirectory = dir;
      lblSaveTo.SetTextAsPathWithEllipsis (dir);

      enableButtonConvert ();
    }

    private void setAaxCopyDirectory () {
      string dir = SetDestinationDirectory (this, Settings.AaxCopyDirectory, R.Audible, this.Text, R.MsgAaxCopyFolder, R.MsgAaxCopyNoFolder);
      if (dir is null)
        return;

      Settings.AaxCopyDirectory = dir;
    }


    private void numUpDnTrkLen_ValueChanged (object sender, EventArgs e) {
      if (_resetFlagTrackLength)
        return;
      Settings.TrkDurMins = (byte)numUpDnTrkLen.Value;
      enableButtonConvert ();
    }

    private void lblSaveTo_SizeChanged (object sender, EventArgs e) {
      setLabelSaveTo ();
    }

    private void listViewAaxFiles_SizeChanged (object sender, EventArgs e) {
      if (_fileItems.Count > 0)
        return;
      _resizeTimer?.Stop ();
      if (_resizeFlag)
        return;
      _resizeTimer?.Start ();
    }

    private void resizeTimer_Tick (object sender, EventArgs e) {
      _resizeTimer.Stop ();
      adjustListViewColumnWidths ();
    }

    #endregion Event Handlers
  }
}
