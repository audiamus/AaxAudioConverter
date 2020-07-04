namespace audiamus.aaxconv {
  partial class MainForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose ();
      }
      base.Dispose (disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent () {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.grpBoxAaxFiles = new System.Windows.Forms.GroupBox();
      this.listViewAaxFiles = new System.Windows.Forms.ListView();
      this.clmHdrAlbum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrArtist = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrYear = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrNarrator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrSampleRate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.clmHdrBitRate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.btnRem = new System.Windows.Forms.Button();
      this.btnAddFile = new System.Windows.Forms.Button();
      this.grpBoxMode = new System.Windows.Forms.GroupBox();
      this.panelTrkLen = new System.Windows.Forms.Panel();
      this.lblDuration = new System.Windows.Forms.Label();
      this.lblTraclLen = new System.Windows.Forms.Label();
      this.numUpDnTrkLen = new System.Windows.Forms.NumericUpDown();
      this.radBtnTimeSplit = new System.Windows.Forms.RadioButton();
      this.radBtnChaptSplit = new System.Windows.Forms.RadioButton();
      this.radBtnChapt = new System.Windows.Forms.RadioButton();
      this.radBtnSingle = new System.Windows.Forms.RadioButton();
      this.panelExec = new System.Windows.Forms.Panel();
      this.lblProgress = new System.Windows.Forms.Label();
      this.btnAbort = new System.Windows.Forms.Button();
      this.progressBarPart = new System.Windows.Forms.ProgressBar();
      this.progressBarTrack = new System.Windows.Forms.ProgressBar();
      this.btnConvert = new System.Windows.Forms.Button();
      this.grpBoxFormat = new System.Windows.Forms.GroupBox();
      this.radBtnMp4 = new System.Windows.Forms.RadioButton();
      this.radBtnMp3 = new System.Windows.Forms.RadioButton();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.btnSaveTo = new System.Windows.Forms.Button();
      this.grpBoxNaming = new System.Windows.Forms.GroupBox();
      this.propGridNaming = new System.Windows.Forms.PropertyGrid();
      this.panelTop = new System.Windows.Forms.Panel();
      this.tableLayoutSettings = new System.Windows.Forms.TableLayoutPanel();
      this.panelSettingsLeft = new System.Windows.Forms.Panel();
      this.grpBoxDest = new System.Windows.Forms.GroupBox();
      this.lblSaveTo = new System.Windows.Forms.Label();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.tsmiDetails = new System.Windows.Forms.ToolStripMenuItem();
      this.tsmiPreview = new System.Windows.Forms.ToolStripMenuItem();
      this.grpBoxAaxFiles.SuspendLayout();
      this.grpBoxMode.SuspendLayout();
      this.panelTrkLen.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numUpDnTrkLen)).BeginInit();
      this.panelExec.SuspendLayout();
      this.grpBoxFormat.SuspendLayout();
      this.grpBoxNaming.SuspendLayout();
      this.panelTop.SuspendLayout();
      this.tableLayoutSettings.SuspendLayout();
      this.panelSettingsLeft.SuspendLayout();
      this.grpBoxDest.SuspendLayout();
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpBoxAaxFiles
      // 
      resources.ApplyResources(this.grpBoxAaxFiles, "grpBoxAaxFiles");
      this.grpBoxAaxFiles.Controls.Add(this.listViewAaxFiles);
      this.grpBoxAaxFiles.Controls.Add(this.btnRem);
      this.grpBoxAaxFiles.Controls.Add(this.btnAddFile);
      this.grpBoxAaxFiles.Name = "grpBoxAaxFiles";
      this.grpBoxAaxFiles.TabStop = false;
      this.toolTip1.SetToolTip(this.grpBoxAaxFiles, resources.GetString("grpBoxAaxFiles.ToolTip"));
      // 
      // listViewAaxFiles
      // 
      resources.ApplyResources(this.listViewAaxFiles, "listViewAaxFiles");
      this.listViewAaxFiles.AllowDrop = true;
      this.listViewAaxFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmHdrAlbum,
            this.clmHdrArtist,
            this.clmHdrSize,
            this.clmHdrDuration,
            this.clmHdrYear,
            this.clmHdrNarrator,
            this.clmHdrSampleRate,
            this.clmHdrBitRate});
      this.listViewAaxFiles.FullRowSelect = true;
      this.listViewAaxFiles.GridLines = true;
      this.listViewAaxFiles.HideSelection = false;
      this.listViewAaxFiles.Name = "listViewAaxFiles";
      this.listViewAaxFiles.SmallImageList = this.imageList1;
      this.toolTip1.SetToolTip(this.listViewAaxFiles, resources.GetString("listViewAaxFiles.ToolTip"));
      this.listViewAaxFiles.UseCompatibleStateImageBehavior = false;
      this.listViewAaxFiles.View = System.Windows.Forms.View.Details;
      this.listViewAaxFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewAaxFiles_ColumnClick);
      this.listViewAaxFiles.SelectedIndexChanged += new System.EventHandler(this.listViewAaxFiles_SelectedIndexChanged);
      this.listViewAaxFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewAaxFiles_DragDrop);
      this.listViewAaxFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewAaxFiles_DragEnter);
      this.listViewAaxFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewAaxFiles_KeyDown);
      this.listViewAaxFiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewAaxFiles_MouseClick);
      // 
      // clmHdrAlbum
      // 
      resources.ApplyResources(this.clmHdrAlbum, "clmHdrAlbum");
      // 
      // clmHdrArtist
      // 
      resources.ApplyResources(this.clmHdrArtist, "clmHdrArtist");
      // 
      // clmHdrSize
      // 
      resources.ApplyResources(this.clmHdrSize, "clmHdrSize");
      // 
      // clmHdrDuration
      // 
      resources.ApplyResources(this.clmHdrDuration, "clmHdrDuration");
      // 
      // clmHdrYear
      // 
      resources.ApplyResources(this.clmHdrYear, "clmHdrYear");
      // 
      // clmHdrNarrator
      // 
      resources.ApplyResources(this.clmHdrNarrator, "clmHdrNarrator");
      // 
      // clmHdrSampleRate
      // 
      resources.ApplyResources(this.clmHdrSampleRate, "clmHdrSampleRate");
      // 
      // clmHdrBitRate
      // 
      resources.ApplyResources(this.clmHdrBitRate, "clmHdrBitRate");
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "blank.png");
      this.imageList1.Images.SetKeyName(1, "added.png");
      // 
      // btnRem
      // 
      resources.ApplyResources(this.btnRem, "btnRem");
      this.btnRem.Name = "btnRem";
      this.toolTip1.SetToolTip(this.btnRem, resources.GetString("btnRem.ToolTip"));
      this.btnRem.UseVisualStyleBackColor = true;
      this.btnRem.Click += new System.EventHandler(this.btnRem_Click);
      // 
      // btnAddFile
      // 
      resources.ApplyResources(this.btnAddFile, "btnAddFile");
      this.btnAddFile.Name = "btnAddFile";
      this.toolTip1.SetToolTip(this.btnAddFile, resources.GetString("btnAddFile.ToolTip"));
      this.btnAddFile.UseVisualStyleBackColor = true;
      this.btnAddFile.Click += new System.EventHandler(this.btnAddFile_Click);
      // 
      // grpBoxMode
      // 
      resources.ApplyResources(this.grpBoxMode, "grpBoxMode");
      this.grpBoxMode.Controls.Add(this.panelTrkLen);
      this.grpBoxMode.Controls.Add(this.radBtnTimeSplit);
      this.grpBoxMode.Controls.Add(this.radBtnChaptSplit);
      this.grpBoxMode.Controls.Add(this.radBtnChapt);
      this.grpBoxMode.Controls.Add(this.radBtnSingle);
      this.grpBoxMode.Name = "grpBoxMode";
      this.grpBoxMode.TabStop = false;
      this.toolTip1.SetToolTip(this.grpBoxMode, resources.GetString("grpBoxMode.ToolTip"));
      // 
      // panelTrkLen
      // 
      resources.ApplyResources(this.panelTrkLen, "panelTrkLen");
      this.panelTrkLen.Controls.Add(this.lblDuration);
      this.panelTrkLen.Controls.Add(this.lblTraclLen);
      this.panelTrkLen.Controls.Add(this.numUpDnTrkLen);
      this.panelTrkLen.Name = "panelTrkLen";
      this.toolTip1.SetToolTip(this.panelTrkLen, resources.GetString("panelTrkLen.ToolTip"));
      // 
      // lblDuration
      // 
      resources.ApplyResources(this.lblDuration, "lblDuration");
      this.lblDuration.Name = "lblDuration";
      this.toolTip1.SetToolTip(this.lblDuration, resources.GetString("lblDuration.ToolTip"));
      // 
      // lblTraclLen
      // 
      resources.ApplyResources(this.lblTraclLen, "lblTraclLen");
      this.lblTraclLen.Name = "lblTraclLen";
      this.toolTip1.SetToolTip(this.lblTraclLen, resources.GetString("lblTraclLen.ToolTip"));
      // 
      // numUpDnTrkLen
      // 
      resources.ApplyResources(this.numUpDnTrkLen, "numUpDnTrkLen");
      this.numUpDnTrkLen.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.numUpDnTrkLen.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
      this.numUpDnTrkLen.Name = "numUpDnTrkLen";
      this.toolTip1.SetToolTip(this.numUpDnTrkLen, resources.GetString("numUpDnTrkLen.ToolTip"));
      this.numUpDnTrkLen.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.numUpDnTrkLen.ValueChanged += new System.EventHandler(this.numUpDnTrkLen_ValueChanged);
      // 
      // radBtnTimeSplit
      // 
      resources.ApplyResources(this.radBtnTimeSplit, "radBtnTimeSplit");
      this.radBtnTimeSplit.Name = "radBtnTimeSplit";
      this.radBtnTimeSplit.TabStop = true;
      this.toolTip1.SetToolTip(this.radBtnTimeSplit, resources.GetString("radBtnTimeSplit.ToolTip"));
      this.radBtnTimeSplit.UseVisualStyleBackColor = true;
      this.radBtnTimeSplit.CheckedChanged += new System.EventHandler(this.radBtnTimeSplit_CheckedChanged);
      // 
      // radBtnChaptSplit
      // 
      resources.ApplyResources(this.radBtnChaptSplit, "radBtnChaptSplit");
      this.radBtnChaptSplit.Name = "radBtnChaptSplit";
      this.radBtnChaptSplit.TabStop = true;
      this.toolTip1.SetToolTip(this.radBtnChaptSplit, resources.GetString("radBtnChaptSplit.ToolTip"));
      this.radBtnChaptSplit.UseVisualStyleBackColor = true;
      this.radBtnChaptSplit.CheckedChanged += new System.EventHandler(this.radBtnChaptSplit_CheckedChanged);
      // 
      // radBtnChapt
      // 
      resources.ApplyResources(this.radBtnChapt, "radBtnChapt");
      this.radBtnChapt.Name = "radBtnChapt";
      this.radBtnChapt.TabStop = true;
      this.toolTip1.SetToolTip(this.radBtnChapt, resources.GetString("radBtnChapt.ToolTip"));
      this.radBtnChapt.UseVisualStyleBackColor = true;
      this.radBtnChapt.CheckedChanged += new System.EventHandler(this.radBtnChapt_CheckedChanged);
      // 
      // radBtnSingle
      // 
      resources.ApplyResources(this.radBtnSingle, "radBtnSingle");
      this.radBtnSingle.Name = "radBtnSingle";
      this.radBtnSingle.TabStop = true;
      this.toolTip1.SetToolTip(this.radBtnSingle, resources.GetString("radBtnSingle.ToolTip"));
      this.radBtnSingle.UseVisualStyleBackColor = true;
      this.radBtnSingle.CheckedChanged += new System.EventHandler(this.radBtnSingle_CheckedChanged);
      // 
      // panelExec
      // 
      resources.ApplyResources(this.panelExec, "panelExec");
      this.panelExec.Controls.Add(this.lblProgress);
      this.panelExec.Controls.Add(this.btnAbort);
      this.panelExec.Controls.Add(this.progressBarPart);
      this.panelExec.Controls.Add(this.progressBarTrack);
      this.panelExec.Controls.Add(this.btnConvert);
      this.panelExec.Name = "panelExec";
      this.toolTip1.SetToolTip(this.panelExec, resources.GetString("panelExec.ToolTip"));
      // 
      // lblProgress
      // 
      resources.ApplyResources(this.lblProgress, "lblProgress");
      this.lblProgress.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblProgress.Name = "lblProgress";
      this.toolTip1.SetToolTip(this.lblProgress, resources.GetString("lblProgress.ToolTip"));
      // 
      // btnAbort
      // 
      resources.ApplyResources(this.btnAbort, "btnAbort");
      this.btnAbort.Name = "btnAbort";
      this.toolTip1.SetToolTip(this.btnAbort, resources.GetString("btnAbort.ToolTip"));
      this.btnAbort.UseVisualStyleBackColor = true;
      this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
      // 
      // progressBarPart
      // 
      resources.ApplyResources(this.progressBarPart, "progressBarPart");
      this.progressBarPart.Maximum = 1000;
      this.progressBarPart.Name = "progressBarPart";
      this.toolTip1.SetToolTip(this.progressBarPart, resources.GetString("progressBarPart.ToolTip"));
      // 
      // progressBarTrack
      // 
      resources.ApplyResources(this.progressBarTrack, "progressBarTrack");
      this.progressBarTrack.Maximum = 1000;
      this.progressBarTrack.Name = "progressBarTrack";
      this.toolTip1.SetToolTip(this.progressBarTrack, resources.GetString("progressBarTrack.ToolTip"));
      // 
      // btnConvert
      // 
      resources.ApplyResources(this.btnConvert, "btnConvert");
      this.btnConvert.Name = "btnConvert";
      this.toolTip1.SetToolTip(this.btnConvert, resources.GetString("btnConvert.ToolTip"));
      this.btnConvert.UseVisualStyleBackColor = true;
      this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
      // 
      // grpBoxFormat
      // 
      resources.ApplyResources(this.grpBoxFormat, "grpBoxFormat");
      this.grpBoxFormat.Controls.Add(this.radBtnMp4);
      this.grpBoxFormat.Controls.Add(this.radBtnMp3);
      this.grpBoxFormat.Name = "grpBoxFormat";
      this.grpBoxFormat.TabStop = false;
      this.toolTip1.SetToolTip(this.grpBoxFormat, resources.GetString("grpBoxFormat.ToolTip"));
      // 
      // radBtnMp4
      // 
      resources.ApplyResources(this.radBtnMp4, "radBtnMp4");
      this.radBtnMp4.Name = "radBtnMp4";
      this.radBtnMp4.TabStop = true;
      this.toolTip1.SetToolTip(this.radBtnMp4, resources.GetString("radBtnMp4.ToolTip"));
      this.radBtnMp4.UseVisualStyleBackColor = true;
      this.radBtnMp4.CheckedChanged += new System.EventHandler(this.radBtnM4A_CheckedChanged);
      // 
      // radBtnMp3
      // 
      resources.ApplyResources(this.radBtnMp3, "radBtnMp3");
      this.radBtnMp3.Name = "radBtnMp3";
      this.radBtnMp3.TabStop = true;
      this.toolTip1.SetToolTip(this.radBtnMp3, resources.GetString("radBtnMp3.ToolTip"));
      this.radBtnMp3.UseVisualStyleBackColor = true;
      this.radBtnMp3.CheckedChanged += new System.EventHandler(this.radBtnMp3_CheckedChanged);
      // 
      // btnSaveTo
      // 
      resources.ApplyResources(this.btnSaveTo, "btnSaveTo");
      this.btnSaveTo.Name = "btnSaveTo";
      this.toolTip1.SetToolTip(this.btnSaveTo, resources.GetString("btnSaveTo.ToolTip"));
      this.btnSaveTo.UseVisualStyleBackColor = true;
      this.btnSaveTo.Click += new System.EventHandler(this.btnSaveTo_Click);
      // 
      // grpBoxNaming
      // 
      resources.ApplyResources(this.grpBoxNaming, "grpBoxNaming");
      this.grpBoxNaming.Controls.Add(this.propGridNaming);
      this.grpBoxNaming.Name = "grpBoxNaming";
      this.grpBoxNaming.TabStop = false;
      this.toolTip1.SetToolTip(this.grpBoxNaming, resources.GetString("grpBoxNaming.ToolTip"));
      // 
      // propGridNaming
      // 
      resources.ApplyResources(this.propGridNaming, "propGridNaming");
      this.propGridNaming.Name = "propGridNaming";
      this.propGridNaming.PropertySort = System.Windows.Forms.PropertySort.NoSort;
      this.propGridNaming.ToolbarVisible = false;
      this.toolTip1.SetToolTip(this.propGridNaming, resources.GetString("propGridNaming.ToolTip"));
      this.propGridNaming.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propGridNaming_PropertyValueChanged);
      // 
      // panelTop
      // 
      resources.ApplyResources(this.panelTop, "panelTop");
      this.panelTop.Controls.Add(this.grpBoxAaxFiles);
      this.panelTop.Controls.Add(this.tableLayoutSettings);
      this.panelTop.Name = "panelTop";
      this.toolTip1.SetToolTip(this.panelTop, resources.GetString("panelTop.ToolTip"));
      // 
      // tableLayoutSettings
      // 
      resources.ApplyResources(this.tableLayoutSettings, "tableLayoutSettings");
      this.tableLayoutSettings.Controls.Add(this.panelSettingsLeft, 0, 0);
      this.tableLayoutSettings.Controls.Add(this.grpBoxNaming, 1, 0);
      this.tableLayoutSettings.Name = "tableLayoutSettings";
      this.toolTip1.SetToolTip(this.tableLayoutSettings, resources.GetString("tableLayoutSettings.ToolTip"));
      // 
      // panelSettingsLeft
      // 
      resources.ApplyResources(this.panelSettingsLeft, "panelSettingsLeft");
      this.panelSettingsLeft.Controls.Add(this.grpBoxMode);
      this.panelSettingsLeft.Controls.Add(this.grpBoxFormat);
      this.panelSettingsLeft.Controls.Add(this.grpBoxDest);
      this.panelSettingsLeft.Name = "panelSettingsLeft";
      this.toolTip1.SetToolTip(this.panelSettingsLeft, resources.GetString("panelSettingsLeft.ToolTip"));
      // 
      // grpBoxDest
      // 
      resources.ApplyResources(this.grpBoxDest, "grpBoxDest");
      this.grpBoxDest.Controls.Add(this.lblSaveTo);
      this.grpBoxDest.Controls.Add(this.btnSaveTo);
      this.grpBoxDest.Name = "grpBoxDest";
      this.grpBoxDest.TabStop = false;
      this.toolTip1.SetToolTip(this.grpBoxDest, resources.GetString("grpBoxDest.ToolTip"));
      // 
      // lblSaveTo
      // 
      resources.ApplyResources(this.lblSaveTo, "lblSaveTo");
      this.lblSaveTo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblSaveTo.Name = "lblSaveTo";
      this.toolTip1.SetToolTip(this.lblSaveTo, resources.GetString("lblSaveTo.ToolTip"));
      // 
      // contextMenuStrip1
      // 
      resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDetails,
            this.tsmiPreview});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.toolTip1.SetToolTip(this.contextMenuStrip1, resources.GetString("contextMenuStrip1.ToolTip"));
      // 
      // tsmiDetails
      // 
      resources.ApplyResources(this.tsmiDetails, "tsmiDetails");
      this.tsmiDetails.Name = "tsmiDetails";
      this.tsmiDetails.Click += new System.EventHandler(this.tsmiDetails_Click);
      // 
      // tsmiPreview
      // 
      resources.ApplyResources(this.tsmiPreview, "tsmiPreview");
      this.tsmiPreview.Name = "tsmiPreview";
      this.tsmiPreview.Click += new System.EventHandler(this.tsmiPreview_Click);
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnAddFile;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panelTop);
      this.Controls.Add(this.panelExec);
      this.KeyPreview = true;
      this.Name = "MainForm";
      this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
      this.grpBoxAaxFiles.ResumeLayout(false);
      this.grpBoxMode.ResumeLayout(false);
      this.grpBoxMode.PerformLayout();
      this.panelTrkLen.ResumeLayout(false);
      this.panelTrkLen.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numUpDnTrkLen)).EndInit();
      this.panelExec.ResumeLayout(false);
      this.grpBoxFormat.ResumeLayout(false);
      this.grpBoxFormat.PerformLayout();
      this.grpBoxNaming.ResumeLayout(false);
      this.panelTop.ResumeLayout(false);
      this.tableLayoutSettings.ResumeLayout(false);
      this.panelSettingsLeft.ResumeLayout(false);
      this.grpBoxDest.ResumeLayout(false);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.GroupBox grpBoxAaxFiles;
    private System.Windows.Forms.ListView listViewAaxFiles;
    private System.Windows.Forms.ColumnHeader clmHdrAlbum;
    private System.Windows.Forms.ColumnHeader clmHdrArtist;
    private System.Windows.Forms.ColumnHeader clmHdrSize;
    private System.Windows.Forms.ColumnHeader clmHdrDuration;
    private System.Windows.Forms.Button btnRem;
    private System.Windows.Forms.Button btnAddFile;
    private System.Windows.Forms.GroupBox grpBoxMode;
    private System.Windows.Forms.RadioButton radBtnChaptSplit;
    private System.Windows.Forms.RadioButton radBtnChapt;
    private System.Windows.Forms.RadioButton radBtnSingle;
    private System.Windows.Forms.Panel panelExec;
    private System.Windows.Forms.Button btnConvert;
    private System.Windows.Forms.Button btnAbort;
    private System.Windows.Forms.ProgressBar progressBarTrack;
    private System.Windows.Forms.Label lblDuration;
    private System.Windows.Forms.NumericUpDown numUpDnTrkLen;
    private System.Windows.Forms.Label lblProgress;
    private System.Windows.Forms.GroupBox grpBoxFormat;
    private System.Windows.Forms.RadioButton radBtnMp4;
    private System.Windows.Forms.RadioButton radBtnMp3;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.ProgressBar progressBarPart;
    private System.Windows.Forms.Button btnSaveTo;
    private System.Windows.Forms.GroupBox grpBoxDest;
    private System.Windows.Forms.Label lblTraclLen;
    private System.Windows.Forms.TableLayoutPanel tableLayoutSettings;
    private System.Windows.Forms.Panel panelSettingsLeft;
    private System.Windows.Forms.GroupBox grpBoxNaming;
    private System.Windows.Forms.PropertyGrid propGridNaming;
    private System.Windows.Forms.Label lblSaveTo;
    private System.Windows.Forms.ColumnHeader clmHdrNarrator;
    private System.Windows.Forms.ColumnHeader clmHdrSampleRate;
    private System.Windows.Forms.ColumnHeader clmHdrBitRate;
    private System.Windows.Forms.ColumnHeader clmHdrYear;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem tsmiDetails;
    private System.Windows.Forms.ToolStripMenuItem tsmiPreview;
    private System.Windows.Forms.RadioButton radBtnTimeSplit;
    private System.Windows.Forms.Panel panelTrkLen;
  }
}

