namespace audiamus.aaxconv {
  partial class SettingsForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.panelRight = new System.Windows.Forms.Panel();
      this.btnReset = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.lblFfmpegLoc = new System.Windows.Forms.Label();
      this.btnFfmpegLoc = new System.Windows.Forms.Button();
      this.lblRegActCode = new System.Windows.Forms.Label();
      this.panelActCode = new System.Windows.Forms.Panel();
      this.listBoxActCode = new System.Windows.Forms.ListBox();
      this.btnRegActCode = new System.Windows.Forms.Button();
      this.lblUsrActCode = new System.Windows.Forms.Label();
      this.btnUsrActCode = new System.Windows.Forms.Button();
      this.lblFileAssoc = new System.Windows.Forms.Label();
      this.ckBoxFileAssoc = new System.Windows.Forms.CheckBox();
      this.lblAaxCopy = new System.Windows.Forms.Label();
      this.panelAaxCopy = new System.Windows.Forms.Panel();
      this.comBoxAaxCopy = new System.Windows.Forms.ComboBox();
      this.btnAaxCopyDir = new System.Windows.Forms.Button();
      this.lblLaunchPlayer = new System.Windows.Forms.Label();
      this.ckBoxLaunchPlayer = new System.Windows.Forms.CheckBox();
      this.lblUpdate = new System.Windows.Forms.Label();
      this.comBoxUpdate = new System.Windows.Forms.ComboBox();
      this.lblLang = new System.Windows.Forms.Label();
      this.comBoxLang = new System.Windows.Forms.ComboBox();
      this.lblPartName = new System.Windows.Forms.Label();
      this.panelPartName = new System.Windows.Forms.Panel();
      this.txtBoxPartName = new System.Windows.Forms.TextBox();
      this.comBoxPartName = new System.Windows.Forms.ComboBox();
      this.lblCustPart = new System.Windows.Forms.Label();
      this.txtBoxCustPart = new System.Windows.Forms.TextBox();
      this.lblCustTitleChars = new System.Windows.Forms.Label();
      this.txtBoxCustTitleChars = new System.Windows.Forms.TextBox();
      this.lblNamedChapters = new System.Windows.Forms.Label();
      this.comBoxNamedChapters = new System.Windows.Forms.ComboBox();
      this.lblFlatFolders = new System.Windows.Forms.Label();
      this.panelFlatFolders = new System.Windows.Forms.Panel();
      this.comBoxFlatFolders = new System.Windows.Forms.ComboBox();
      this.ckBoxFlatFolders = new System.Windows.Forms.CheckBox();
      this.lblShipShort = new System.Windows.Forms.Label();
      this.panelSkipShort = new System.Windows.Forms.Panel();
      this.lblShSecDur = new System.Windows.Forms.Label();
      this.nudShortChapter = new System.Windows.Forms.NumericUpDown();
      this.lblSkipVeryShort = new System.Windows.Forms.Label();
      this.panelSkipVeryShort = new System.Windows.Forms.Panel();
      this.lblVyShSecDur = new System.Windows.Forms.Label();
      this.nudVeryShortChapter = new System.Windows.Forms.NumericUpDown();
      this.lblVerAdjChapters = new System.Windows.Forms.Label();
      this.comBoxVerAdjChapters = new System.Windows.Forms.ComboBox();
      this.lblExtraMetaFiles = new System.Windows.Forms.Label();
      this.ckBoxExtraMetaFiles = new System.Windows.Forms.CheckBox();
      this.lblM4B = new System.Windows.Forms.Label();
      this.comBoxM4B = new System.Windows.Forms.ComboBox();
      this.lblLatin1 = new System.Windows.Forms.Label();
      this.ckBoxLatin1 = new System.Windows.Forms.CheckBox();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.ckBoxIntermedCopySingle = new System.Windows.Forms.CheckBox();
      this.comBoxFixAacEncoding = new System.Windows.Forms.ComboBox();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.lblIntermedCopySingle = new System.Windows.Forms.Label();
      this.lblFixAacEnc = new System.Windows.Forms.Label();
      this.panelRight.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.panelActCode.SuspendLayout();
      this.panelAaxCopy.SuspendLayout();
      this.panelPartName.SuspendLayout();
      this.panelFlatFolders.SuspendLayout();
      this.panelSkipShort.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudShortChapter)).BeginInit();
      this.panelSkipVeryShort.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudVeryShortChapter)).BeginInit();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panelRight
      // 
      resources.ApplyResources(this.panelRight, "panelRight");
      this.panelRight.Controls.Add(this.btnReset);
      this.panelRight.Controls.Add(this.btnCancel);
      this.panelRight.Controls.Add(this.btnOK);
      this.panelRight.Name = "panelRight";
      this.toolTip1.SetToolTip(this.panelRight, resources.GetString("panelRight.ToolTip"));
      // 
      // btnReset
      // 
      resources.ApplyResources(this.btnReset, "btnReset");
      this.btnReset.Name = "btnReset";
      this.toolTip1.SetToolTip(this.btnReset, resources.GetString("btnReset.ToolTip"));
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.toolTip1.SetToolTip(this.btnCancel, resources.GetString("btnCancel.ToolTip"));
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Name = "btnOK";
      this.toolTip1.SetToolTip(this.btnOK, resources.GetString("btnOK.ToolTip"));
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.lblFfmpegLoc, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnFfmpegLoc, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblRegActCode, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.panelActCode, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.lblUsrActCode, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.btnUsrActCode, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.lblFileAssoc, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxFileAssoc, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.lblAaxCopy, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.panelAaxCopy, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.lblLaunchPlayer, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxLaunchPlayer, 1, 5);
      this.tableLayoutPanel1.Controls.Add(this.lblUpdate, 0, 6);
      this.tableLayoutPanel1.Controls.Add(this.comBoxUpdate, 1, 6);
      this.tableLayoutPanel1.Controls.Add(this.lblLang, 0, 7);
      this.tableLayoutPanel1.Controls.Add(this.comBoxLang, 1, 7);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.toolTip1.SetToolTip(this.tableLayoutPanel1, resources.GetString("tableLayoutPanel1.ToolTip"));
      // 
      // lblFfmpegLoc
      // 
      resources.ApplyResources(this.lblFfmpegLoc, "lblFfmpegLoc");
      this.lblFfmpegLoc.Name = "lblFfmpegLoc";
      this.toolTip1.SetToolTip(this.lblFfmpegLoc, resources.GetString("lblFfmpegLoc.ToolTip"));
      // 
      // btnFfmpegLoc
      // 
      resources.ApplyResources(this.btnFfmpegLoc, "btnFfmpegLoc");
      this.btnFfmpegLoc.Name = "btnFfmpegLoc";
      this.toolTip1.SetToolTip(this.btnFfmpegLoc, resources.GetString("btnFfmpegLoc.ToolTip"));
      this.btnFfmpegLoc.UseVisualStyleBackColor = true;
      this.btnFfmpegLoc.Click += new System.EventHandler(this.btnFfmpegLoc_Click);
      // 
      // lblRegActCode
      // 
      resources.ApplyResources(this.lblRegActCode, "lblRegActCode");
      this.lblRegActCode.Name = "lblRegActCode";
      this.toolTip1.SetToolTip(this.lblRegActCode, resources.GetString("lblRegActCode.ToolTip"));
      // 
      // panelActCode
      // 
      resources.ApplyResources(this.panelActCode, "panelActCode");
      this.panelActCode.Controls.Add(this.listBoxActCode);
      this.panelActCode.Controls.Add(this.btnRegActCode);
      this.panelActCode.Name = "panelActCode";
      this.toolTip1.SetToolTip(this.panelActCode, resources.GetString("panelActCode.ToolTip"));
      // 
      // listBoxActCode
      // 
      resources.ApplyResources(this.listBoxActCode, "listBoxActCode");
      this.listBoxActCode.FormattingEnabled = true;
      this.listBoxActCode.Name = "listBoxActCode";
      this.listBoxActCode.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.toolTip1.SetToolTip(this.listBoxActCode, resources.GetString("listBoxActCode.ToolTip"));
      // 
      // btnRegActCode
      // 
      resources.ApplyResources(this.btnRegActCode, "btnRegActCode");
      this.btnRegActCode.Name = "btnRegActCode";
      this.toolTip1.SetToolTip(this.btnRegActCode, resources.GetString("btnRegActCode.ToolTip"));
      this.btnRegActCode.UseVisualStyleBackColor = true;
      this.btnRegActCode.Click += new System.EventHandler(this.btnRegActCode_Click);
      // 
      // lblUsrActCode
      // 
      resources.ApplyResources(this.lblUsrActCode, "lblUsrActCode");
      this.lblUsrActCode.Name = "lblUsrActCode";
      this.toolTip1.SetToolTip(this.lblUsrActCode, resources.GetString("lblUsrActCode.ToolTip"));
      // 
      // btnUsrActCode
      // 
      resources.ApplyResources(this.btnUsrActCode, "btnUsrActCode");
      this.btnUsrActCode.Name = "btnUsrActCode";
      this.toolTip1.SetToolTip(this.btnUsrActCode, resources.GetString("btnUsrActCode.ToolTip"));
      this.btnUsrActCode.UseVisualStyleBackColor = true;
      this.btnUsrActCode.Click += new System.EventHandler(this.btnUsrActCode_Click);
      // 
      // lblFileAssoc
      // 
      resources.ApplyResources(this.lblFileAssoc, "lblFileAssoc");
      this.lblFileAssoc.Name = "lblFileAssoc";
      this.toolTip1.SetToolTip(this.lblFileAssoc, resources.GetString("lblFileAssoc.ToolTip"));
      // 
      // ckBoxFileAssoc
      // 
      resources.ApplyResources(this.ckBoxFileAssoc, "ckBoxFileAssoc");
      this.ckBoxFileAssoc.Name = "ckBoxFileAssoc";
      this.toolTip1.SetToolTip(this.ckBoxFileAssoc, resources.GetString("ckBoxFileAssoc.ToolTip"));
      this.ckBoxFileAssoc.UseVisualStyleBackColor = true;
      // 
      // lblAaxCopy
      // 
      resources.ApplyResources(this.lblAaxCopy, "lblAaxCopy");
      this.lblAaxCopy.Name = "lblAaxCopy";
      this.toolTip1.SetToolTip(this.lblAaxCopy, resources.GetString("lblAaxCopy.ToolTip"));
      // 
      // panelAaxCopy
      // 
      resources.ApplyResources(this.panelAaxCopy, "panelAaxCopy");
      this.panelAaxCopy.Controls.Add(this.comBoxAaxCopy);
      this.panelAaxCopy.Controls.Add(this.btnAaxCopyDir);
      this.panelAaxCopy.Name = "panelAaxCopy";
      this.toolTip1.SetToolTip(this.panelAaxCopy, resources.GetString("panelAaxCopy.ToolTip"));
      // 
      // comBoxAaxCopy
      // 
      resources.ApplyResources(this.comBoxAaxCopy, "comBoxAaxCopy");
      this.comBoxAaxCopy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxAaxCopy.FormattingEnabled = true;
      this.comBoxAaxCopy.Name = "comBoxAaxCopy";
      this.toolTip1.SetToolTip(this.comBoxAaxCopy, resources.GetString("comBoxAaxCopy.ToolTip"));
      this.comBoxAaxCopy.SelectedIndexChanged += new System.EventHandler(this.comBoxAaxCopy_SelectedIndexChanged);
      // 
      // btnAaxCopyDir
      // 
      resources.ApplyResources(this.btnAaxCopyDir, "btnAaxCopyDir");
      this.btnAaxCopyDir.Name = "btnAaxCopyDir";
      this.toolTip1.SetToolTip(this.btnAaxCopyDir, resources.GetString("btnAaxCopyDir.ToolTip"));
      this.btnAaxCopyDir.UseVisualStyleBackColor = true;
      this.btnAaxCopyDir.Click += new System.EventHandler(this.btnAaxCopyDir_Click);
      // 
      // lblLaunchPlayer
      // 
      resources.ApplyResources(this.lblLaunchPlayer, "lblLaunchPlayer");
      this.lblLaunchPlayer.Name = "lblLaunchPlayer";
      this.toolTip1.SetToolTip(this.lblLaunchPlayer, resources.GetString("lblLaunchPlayer.ToolTip"));
      // 
      // ckBoxLaunchPlayer
      // 
      resources.ApplyResources(this.ckBoxLaunchPlayer, "ckBoxLaunchPlayer");
      this.ckBoxLaunchPlayer.Name = "ckBoxLaunchPlayer";
      this.toolTip1.SetToolTip(this.ckBoxLaunchPlayer, resources.GetString("ckBoxLaunchPlayer.ToolTip"));
      this.ckBoxLaunchPlayer.UseVisualStyleBackColor = true;
      // 
      // lblUpdate
      // 
      resources.ApplyResources(this.lblUpdate, "lblUpdate");
      this.lblUpdate.Name = "lblUpdate";
      this.toolTip1.SetToolTip(this.lblUpdate, resources.GetString("lblUpdate.ToolTip"));
      // 
      // comBoxUpdate
      // 
      resources.ApplyResources(this.comBoxUpdate, "comBoxUpdate");
      this.comBoxUpdate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxUpdate.FormattingEnabled = true;
      this.comBoxUpdate.Items.AddRange(new object[] {
            resources.GetString("comBoxUpdate.Items"),
            resources.GetString("comBoxUpdate.Items1"),
            resources.GetString("comBoxUpdate.Items2")});
      this.comBoxUpdate.Name = "comBoxUpdate";
      this.toolTip1.SetToolTip(this.comBoxUpdate, resources.GetString("comBoxUpdate.ToolTip"));
      // 
      // lblLang
      // 
      resources.ApplyResources(this.lblLang, "lblLang");
      this.lblLang.Name = "lblLang";
      this.toolTip1.SetToolTip(this.lblLang, resources.GetString("lblLang.ToolTip"));
      // 
      // comBoxLang
      // 
      resources.ApplyResources(this.comBoxLang, "comBoxLang");
      this.comBoxLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxLang.FormattingEnabled = true;
      this.comBoxLang.Items.AddRange(new object[] {
            resources.GetString("comBoxLang.Items")});
      this.comBoxLang.Name = "comBoxLang";
      this.toolTip1.SetToolTip(this.comBoxLang, resources.GetString("comBoxLang.ToolTip"));
      // 
      // lblPartName
      // 
      resources.ApplyResources(this.lblPartName, "lblPartName");
      this.lblPartName.Name = "lblPartName";
      this.toolTip1.SetToolTip(this.lblPartName, resources.GetString("lblPartName.ToolTip"));
      // 
      // panelPartName
      // 
      resources.ApplyResources(this.panelPartName, "panelPartName");
      this.panelPartName.Controls.Add(this.txtBoxPartName);
      this.panelPartName.Controls.Add(this.comBoxPartName);
      this.panelPartName.Name = "panelPartName";
      this.toolTip1.SetToolTip(this.panelPartName, resources.GetString("panelPartName.ToolTip"));
      // 
      // txtBoxPartName
      // 
      resources.ApplyResources(this.txtBoxPartName, "txtBoxPartName");
      this.txtBoxPartName.Name = "txtBoxPartName";
      this.toolTip1.SetToolTip(this.txtBoxPartName, resources.GetString("txtBoxPartName.ToolTip"));
      // 
      // comBoxPartName
      // 
      resources.ApplyResources(this.comBoxPartName, "comBoxPartName");
      this.comBoxPartName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxPartName.FormattingEnabled = true;
      this.comBoxPartName.Name = "comBoxPartName";
      this.toolTip1.SetToolTip(this.comBoxPartName, resources.GetString("comBoxPartName.ToolTip"));
      this.comBoxPartName.SelectedIndexChanged += new System.EventHandler(this.comBoxPartName_SelectedIndexChanged);
      // 
      // lblCustPart
      // 
      resources.ApplyResources(this.lblCustPart, "lblCustPart");
      this.lblCustPart.Name = "lblCustPart";
      this.toolTip1.SetToolTip(this.lblCustPart, resources.GetString("lblCustPart.ToolTip"));
      // 
      // txtBoxCustPart
      // 
      resources.ApplyResources(this.txtBoxCustPart, "txtBoxCustPart");
      this.txtBoxCustPart.Name = "txtBoxCustPart";
      this.toolTip1.SetToolTip(this.txtBoxCustPart, resources.GetString("txtBoxCustPart.ToolTip"));
      this.txtBoxCustPart.Leave += new System.EventHandler(this.txtBoxCustPart_Leave);
      // 
      // lblCustTitleChars
      // 
      resources.ApplyResources(this.lblCustTitleChars, "lblCustTitleChars");
      this.lblCustTitleChars.Name = "lblCustTitleChars";
      this.toolTip1.SetToolTip(this.lblCustTitleChars, resources.GetString("lblCustTitleChars.ToolTip"));
      // 
      // txtBoxCustTitleChars
      // 
      resources.ApplyResources(this.txtBoxCustTitleChars, "txtBoxCustTitleChars");
      this.txtBoxCustTitleChars.Name = "txtBoxCustTitleChars";
      this.toolTip1.SetToolTip(this.txtBoxCustTitleChars, resources.GetString("txtBoxCustTitleChars.ToolTip"));
      this.txtBoxCustTitleChars.TextChanged += new System.EventHandler(this.txtBoxCustTitleChars_TextChanged);
      // 
      // lblNamedChapters
      // 
      resources.ApplyResources(this.lblNamedChapters, "lblNamedChapters");
      this.lblNamedChapters.Name = "lblNamedChapters";
      this.toolTip1.SetToolTip(this.lblNamedChapters, resources.GetString("lblNamedChapters.ToolTip"));
      // 
      // comBoxNamedChapters
      // 
      resources.ApplyResources(this.comBoxNamedChapters, "comBoxNamedChapters");
      this.comBoxNamedChapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxNamedChapters.FormattingEnabled = true;
      this.comBoxNamedChapters.Items.AddRange(new object[] {
            resources.GetString("comBoxNamedChapters.Items"),
            resources.GetString("comBoxNamedChapters.Items1"),
            resources.GetString("comBoxNamedChapters.Items2")});
      this.comBoxNamedChapters.Name = "comBoxNamedChapters";
      this.toolTip1.SetToolTip(this.comBoxNamedChapters, resources.GetString("comBoxNamedChapters.ToolTip"));
      this.comBoxNamedChapters.SelectedIndexChanged += new System.EventHandler(this.comBoxPartName_SelectedIndexChanged);
      // 
      // lblFlatFolders
      // 
      resources.ApplyResources(this.lblFlatFolders, "lblFlatFolders");
      this.lblFlatFolders.Name = "lblFlatFolders";
      this.toolTip1.SetToolTip(this.lblFlatFolders, resources.GetString("lblFlatFolders.ToolTip"));
      // 
      // panelFlatFolders
      // 
      resources.ApplyResources(this.panelFlatFolders, "panelFlatFolders");
      this.panelFlatFolders.Controls.Add(this.comBoxFlatFolders);
      this.panelFlatFolders.Controls.Add(this.ckBoxFlatFolders);
      this.panelFlatFolders.Name = "panelFlatFolders";
      this.toolTip1.SetToolTip(this.panelFlatFolders, resources.GetString("panelFlatFolders.ToolTip"));
      // 
      // comBoxFlatFolders
      // 
      resources.ApplyResources(this.comBoxFlatFolders, "comBoxFlatFolders");
      this.comBoxFlatFolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxFlatFolders.FormattingEnabled = true;
      this.comBoxFlatFolders.Name = "comBoxFlatFolders";
      this.toolTip1.SetToolTip(this.comBoxFlatFolders, resources.GetString("comBoxFlatFolders.ToolTip"));
      // 
      // ckBoxFlatFolders
      // 
      resources.ApplyResources(this.ckBoxFlatFolders, "ckBoxFlatFolders");
      this.ckBoxFlatFolders.Name = "ckBoxFlatFolders";
      this.toolTip1.SetToolTip(this.ckBoxFlatFolders, resources.GetString("ckBoxFlatFolders.ToolTip"));
      this.ckBoxFlatFolders.UseVisualStyleBackColor = true;
      this.ckBoxFlatFolders.CheckedChanged += new System.EventHandler(this.ckBoxFlatFolders_CheckedChanged);
      // 
      // lblShipShort
      // 
      resources.ApplyResources(this.lblShipShort, "lblShipShort");
      this.lblShipShort.Name = "lblShipShort";
      this.toolTip1.SetToolTip(this.lblShipShort, resources.GetString("lblShipShort.ToolTip"));
      // 
      // panelSkipShort
      // 
      resources.ApplyResources(this.panelSkipShort, "panelSkipShort");
      this.panelSkipShort.Controls.Add(this.lblShSecDur);
      this.panelSkipShort.Controls.Add(this.nudShortChapter);
      this.panelSkipShort.Name = "panelSkipShort";
      this.toolTip1.SetToolTip(this.panelSkipShort, resources.GetString("panelSkipShort.ToolTip"));
      // 
      // lblShSecDur
      // 
      resources.ApplyResources(this.lblShSecDur, "lblShSecDur");
      this.lblShSecDur.Name = "lblShSecDur";
      this.toolTip1.SetToolTip(this.lblShSecDur, resources.GetString("lblShSecDur.ToolTip"));
      // 
      // nudShortChapter
      // 
      resources.ApplyResources(this.nudShortChapter, "nudShortChapter");
      this.nudShortChapter.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.nudShortChapter.Name = "nudShortChapter";
      this.toolTip1.SetToolTip(this.nudShortChapter, resources.GetString("nudShortChapter.ToolTip"));
      // 
      // lblSkipVeryShort
      // 
      resources.ApplyResources(this.lblSkipVeryShort, "lblSkipVeryShort");
      this.lblSkipVeryShort.Name = "lblSkipVeryShort";
      this.toolTip1.SetToolTip(this.lblSkipVeryShort, resources.GetString("lblSkipVeryShort.ToolTip"));
      // 
      // panelSkipVeryShort
      // 
      resources.ApplyResources(this.panelSkipVeryShort, "panelSkipVeryShort");
      this.panelSkipVeryShort.Controls.Add(this.lblVyShSecDur);
      this.panelSkipVeryShort.Controls.Add(this.nudVeryShortChapter);
      this.panelSkipVeryShort.Name = "panelSkipVeryShort";
      this.toolTip1.SetToolTip(this.panelSkipVeryShort, resources.GetString("panelSkipVeryShort.ToolTip"));
      // 
      // lblVyShSecDur
      // 
      resources.ApplyResources(this.lblVyShSecDur, "lblVyShSecDur");
      this.lblVyShSecDur.Name = "lblVyShSecDur";
      this.toolTip1.SetToolTip(this.lblVyShSecDur, resources.GetString("lblVyShSecDur.ToolTip"));
      // 
      // nudVeryShortChapter
      // 
      resources.ApplyResources(this.nudVeryShortChapter, "nudVeryShortChapter");
      this.nudVeryShortChapter.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudVeryShortChapter.Name = "nudVeryShortChapter";
      this.toolTip1.SetToolTip(this.nudVeryShortChapter, resources.GetString("nudVeryShortChapter.ToolTip"));
      // 
      // lblVerAdjChapters
      // 
      resources.ApplyResources(this.lblVerAdjChapters, "lblVerAdjChapters");
      this.lblVerAdjChapters.Name = "lblVerAdjChapters";
      this.toolTip1.SetToolTip(this.lblVerAdjChapters, resources.GetString("lblVerAdjChapters.ToolTip"));
      // 
      // comBoxVerAdjChapters
      // 
      resources.ApplyResources(this.comBoxVerAdjChapters, "comBoxVerAdjChapters");
      this.comBoxVerAdjChapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxVerAdjChapters.FormattingEnabled = true;
      this.comBoxVerAdjChapters.Items.AddRange(new object[] {
            resources.GetString("comBoxVerAdjChapters.Items"),
            resources.GetString("comBoxVerAdjChapters.Items1"),
            resources.GetString("comBoxVerAdjChapters.Items2")});
      this.comBoxVerAdjChapters.Name = "comBoxVerAdjChapters";
      this.toolTip1.SetToolTip(this.comBoxVerAdjChapters, resources.GetString("comBoxVerAdjChapters.ToolTip"));
      this.comBoxVerAdjChapters.SelectedIndexChanged += new System.EventHandler(this.comBoxPartName_SelectedIndexChanged);
      // 
      // lblExtraMetaFiles
      // 
      resources.ApplyResources(this.lblExtraMetaFiles, "lblExtraMetaFiles");
      this.lblExtraMetaFiles.Name = "lblExtraMetaFiles";
      this.toolTip1.SetToolTip(this.lblExtraMetaFiles, resources.GetString("lblExtraMetaFiles.ToolTip"));
      // 
      // ckBoxExtraMetaFiles
      // 
      resources.ApplyResources(this.ckBoxExtraMetaFiles, "ckBoxExtraMetaFiles");
      this.ckBoxExtraMetaFiles.Name = "ckBoxExtraMetaFiles";
      this.toolTip1.SetToolTip(this.ckBoxExtraMetaFiles, resources.GetString("ckBoxExtraMetaFiles.ToolTip"));
      this.ckBoxExtraMetaFiles.UseVisualStyleBackColor = true;
      // 
      // lblM4B
      // 
      resources.ApplyResources(this.lblM4B, "lblM4B");
      this.lblM4B.Name = "lblM4B";
      this.toolTip1.SetToolTip(this.lblM4B, resources.GetString("lblM4B.ToolTip"));
      // 
      // comBoxM4B
      // 
      resources.ApplyResources(this.comBoxM4B, "comBoxM4B");
      this.comBoxM4B.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxM4B.FormattingEnabled = true;
      this.comBoxM4B.Items.AddRange(new object[] {
            resources.GetString("comBoxM4B.Items"),
            resources.GetString("comBoxM4B.Items1")});
      this.comBoxM4B.Name = "comBoxM4B";
      this.toolTip1.SetToolTip(this.comBoxM4B, resources.GetString("comBoxM4B.ToolTip"));
      this.comBoxM4B.SelectedIndexChanged += new System.EventHandler(this.comBoxPartName_SelectedIndexChanged);
      // 
      // lblLatin1
      // 
      resources.ApplyResources(this.lblLatin1, "lblLatin1");
      this.lblLatin1.Name = "lblLatin1";
      this.toolTip1.SetToolTip(this.lblLatin1, resources.GetString("lblLatin1.ToolTip"));
      // 
      // ckBoxLatin1
      // 
      resources.ApplyResources(this.ckBoxLatin1, "ckBoxLatin1");
      this.ckBoxLatin1.Name = "ckBoxLatin1";
      this.toolTip1.SetToolTip(this.ckBoxLatin1, resources.GetString("ckBoxLatin1.ToolTip"));
      this.ckBoxLatin1.UseVisualStyleBackColor = true;
      // 
      // ckBoxIntermedCopySingle
      // 
      resources.ApplyResources(this.ckBoxIntermedCopySingle, "ckBoxIntermedCopySingle");
      this.ckBoxIntermedCopySingle.Name = "ckBoxIntermedCopySingle";
      this.toolTip1.SetToolTip(this.ckBoxIntermedCopySingle, resources.GetString("ckBoxIntermedCopySingle.ToolTip"));
      this.ckBoxIntermedCopySingle.UseVisualStyleBackColor = true;
      // 
      // comBoxFixAacEncoding
      // 
      resources.ApplyResources(this.comBoxFixAacEncoding, "comBoxFixAacEncoding");
      this.comBoxFixAacEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxFixAacEncoding.FormattingEnabled = true;
      this.comBoxFixAacEncoding.Items.AddRange(new object[] {
            resources.GetString("comBoxFixAacEncoding.Items"),
            resources.GetString("comBoxFixAacEncoding.Items1"),
            resources.GetString("comBoxFixAacEncoding.Items2")});
      this.comBoxFixAacEncoding.Name = "comBoxFixAacEncoding";
      this.toolTip1.SetToolTip(this.comBoxFixAacEncoding, resources.GetString("comBoxFixAacEncoding.ToolTip"));
      this.comBoxFixAacEncoding.SelectedIndexChanged += new System.EventHandler(this.comBoxPartName_SelectedIndexChanged);
      // 
      // tabControl1
      // 
      resources.ApplyResources(this.tabControl1, "tabControl1");
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.toolTip1.SetToolTip(this.tabControl1, resources.GetString("tabControl1.ToolTip"));
      // 
      // tabPage1
      // 
      resources.ApplyResources(this.tabPage1, "tabPage1");
      this.tabPage1.Controls.Add(this.tableLayoutPanel1);
      this.tabPage1.Name = "tabPage1";
      this.toolTip1.SetToolTip(this.tabPage1, resources.GetString("tabPage1.ToolTip"));
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      resources.ApplyResources(this.tabPage2, "tabPage2");
      this.tabPage2.Controls.Add(this.tableLayoutPanel2);
      this.tabPage2.Name = "tabPage2";
      this.toolTip1.SetToolTip(this.tabPage2, resources.GetString("tabPage2.ToolTip"));
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel2
      // 
      resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
      this.tableLayoutPanel2.Controls.Add(this.lblFlatFolders, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.panelFlatFolders, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.lblPartName, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.panelPartName, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.lblCustPart, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.txtBoxCustPart, 1, 2);
      this.tableLayoutPanel2.Controls.Add(this.lblCustTitleChars, 0, 3);
      this.tableLayoutPanel2.Controls.Add(this.txtBoxCustTitleChars, 1, 3);
      this.tableLayoutPanel2.Controls.Add(this.lblNamedChapters, 0, 4);
      this.tableLayoutPanel2.Controls.Add(this.comBoxNamedChapters, 1, 4);
      this.tableLayoutPanel2.Controls.Add(this.lblShipShort, 0, 5);
      this.tableLayoutPanel2.Controls.Add(this.panelSkipShort, 1, 5);
      this.tableLayoutPanel2.Controls.Add(this.lblSkipVeryShort, 0, 6);
      this.tableLayoutPanel2.Controls.Add(this.panelSkipVeryShort, 1, 6);
      this.tableLayoutPanel2.Controls.Add(this.lblVerAdjChapters, 0, 7);
      this.tableLayoutPanel2.Controls.Add(this.comBoxVerAdjChapters, 1, 7);
      this.tableLayoutPanel2.Controls.Add(this.lblIntermedCopySingle, 0, 8);
      this.tableLayoutPanel2.Controls.Add(this.ckBoxIntermedCopySingle, 1, 8);
      this.tableLayoutPanel2.Controls.Add(this.lblFixAacEnc, 0, 9);
      this.tableLayoutPanel2.Controls.Add(this.comBoxFixAacEncoding, 1, 9);
      this.tableLayoutPanel2.Controls.Add(this.lblM4B, 0, 10);
      this.tableLayoutPanel2.Controls.Add(this.comBoxM4B, 1, 10);
      this.tableLayoutPanel2.Controls.Add(this.lblLatin1, 0, 11);
      this.tableLayoutPanel2.Controls.Add(this.ckBoxLatin1, 1, 11);
      this.tableLayoutPanel2.Controls.Add(this.lblExtraMetaFiles, 0, 12);
      this.tableLayoutPanel2.Controls.Add(this.ckBoxExtraMetaFiles, 1, 12);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.toolTip1.SetToolTip(this.tableLayoutPanel2, resources.GetString("tableLayoutPanel2.ToolTip"));
      // 
      // lblIntermedCopySingle
      // 
      resources.ApplyResources(this.lblIntermedCopySingle, "lblIntermedCopySingle");
      this.lblIntermedCopySingle.Name = "lblIntermedCopySingle";
      this.toolTip1.SetToolTip(this.lblIntermedCopySingle, resources.GetString("lblIntermedCopySingle.ToolTip"));
      // 
      // lblFixAacEnc
      // 
      resources.ApplyResources(this.lblFixAacEnc, "lblFixAacEnc");
      this.lblFixAacEnc.Name = "lblFixAacEnc";
      this.toolTip1.SetToolTip(this.lblFixAacEnc, resources.GetString("lblFixAacEnc.ToolTip"));
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.btnOK;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.panelRight);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SettingsForm";
      this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
      this.panelRight.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.panelActCode.ResumeLayout(false);
      this.panelAaxCopy.ResumeLayout(false);
      this.panelPartName.ResumeLayout(false);
      this.panelPartName.PerformLayout();
      this.panelFlatFolders.ResumeLayout(false);
      this.panelSkipShort.ResumeLayout(false);
      this.panelSkipShort.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudShortChapter)).EndInit();
      this.panelSkipVeryShort.ResumeLayout(false);
      this.panelSkipVeryShort.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudVeryShortChapter)).EndInit();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panelRight;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.CheckBox ckBoxFileAssoc;
    private System.Windows.Forms.Label lblFileAssoc;
    private System.Windows.Forms.Label lblCustPart;
    private System.Windows.Forms.Label lblUsrActCode;
    private System.Windows.Forms.Label lblFfmpegLoc;
    private System.Windows.Forms.Button btnRegActCode;
    private System.Windows.Forms.Button btnUsrActCode;
    private System.Windows.Forms.Button btnFfmpegLoc;
    private System.Windows.Forms.TextBox txtBoxCustPart;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.Label lblLang;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Panel panelActCode;
    private System.Windows.Forms.ListBox listBoxActCode;
    private System.Windows.Forms.Label lblCustTitleChars;
    private System.Windows.Forms.TextBox txtBoxCustTitleChars;
    private System.Windows.Forms.Panel panelPartName;
    private System.Windows.Forms.TextBox txtBoxPartName;
    private System.Windows.Forms.ComboBox comBoxPartName;
    private System.Windows.Forms.Label lblUpdate;
    private System.Windows.Forms.ComboBox comBoxUpdate;
    private System.Windows.Forms.Panel panelFlatFolders;
    private System.Windows.Forms.ComboBox comBoxFlatFolders;
    private System.Windows.Forms.CheckBox ckBoxFlatFolders;
    private System.Windows.Forms.CheckBox ckBoxExtraMetaFiles;
    private System.Windows.Forms.Label lblExtraMetaFiles;
    private System.Windows.Forms.Label lblFlatFolders;
    private System.Windows.Forms.Panel panelSkipShort;
    private System.Windows.Forms.Label lblShSecDur;
    private System.Windows.Forms.NumericUpDown nudShortChapter;
    private System.Windows.Forms.Panel panelSkipVeryShort;
    private System.Windows.Forms.Label lblVyShSecDur;
    private System.Windows.Forms.NumericUpDown nudVeryShortChapter;
    private System.Windows.Forms.Label lblShipShort;
    private System.Windows.Forms.Label lblSkipVeryShort;
    private System.Windows.Forms.CheckBox ckBoxLatin1;
    private System.Windows.Forms.Label lblLatin1;
    private System.Windows.Forms.ComboBox comBoxNamedChapters;
    private System.Windows.Forms.Label lblNamedChapters;
    private System.Windows.Forms.Label lblRegActCode;
    private System.Windows.Forms.Label lblPartName;
    private System.Windows.Forms.ComboBox comBoxLang;
    private System.Windows.Forms.CheckBox ckBoxLaunchPlayer;
    private System.Windows.Forms.Label lblLaunchPlayer;
    private System.Windows.Forms.ComboBox comBoxM4B;
    private System.Windows.Forms.Label lblM4B;
    private System.Windows.Forms.Label lblAaxCopy;
    private System.Windows.Forms.Panel panelAaxCopy;
    private System.Windows.Forms.ComboBox comBoxAaxCopy;
    private System.Windows.Forms.Button btnAaxCopyDir;
    private System.Windows.Forms.ComboBox comBoxVerAdjChapters;
    private System.Windows.Forms.Label lblVerAdjChapters;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label lblIntermedCopySingle;
    private System.Windows.Forms.CheckBox ckBoxIntermedCopySingle;
    private System.Windows.Forms.Label lblFixAacEnc;
    private System.Windows.Forms.ComboBox comBoxFixAacEncoding;
  }
}