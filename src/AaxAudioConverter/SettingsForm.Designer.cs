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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnReset = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.lblUsrActCode = new System.Windows.Forms.Label();
      this.lblFfmpegLoc = new System.Windows.Forms.Label();
      this.btnUsrActCode = new System.Windows.Forms.Button();
      this.btnFfmpegLoc = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.listBoxActCode = new System.Windows.Forms.ListBox();
      this.btnRegActCode = new System.Windows.Forms.Button();
      this.lblCustTitleChars = new System.Windows.Forms.Label();
      this.txtBoxCustTitleChars = new System.Windows.Forms.TextBox();
      this.txtBoxCustPart = new System.Windows.Forms.TextBox();
      this.lblCustPart = new System.Windows.Forms.Label();
      this.panel3 = new System.Windows.Forms.Panel();
      this.txtBoxPartName = new System.Windows.Forms.TextBox();
      this.comBoxPartName = new System.Windows.Forms.ComboBox();
      this.panel6 = new System.Windows.Forms.Panel();
      this.label2 = new System.Windows.Forms.Label();
      this.nudVeryShortChapter = new System.Windows.Forms.NumericUpDown();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.panel5 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.nudShortChapter = new System.Windows.Forms.NumericUpDown();
      this.lblFlatFolders = new System.Windows.Forms.Label();
      this.panel4 = new System.Windows.Forms.Panel();
      this.comBoxFlatFolders = new System.Windows.Forms.ComboBox();
      this.ckBoxFlatFolders = new System.Windows.Forms.CheckBox();
      this.comBoxNamedChapters = new System.Windows.Forms.ComboBox();
      this.lblNamedChapters = new System.Windows.Forms.Label();
      this.lblRegActCode = new System.Windows.Forms.Label();
      this.lblPartName = new System.Windows.Forms.Label();
      this.comBoxLang = new System.Windows.Forms.ComboBox();
      this.comBoxUpdate = new System.Windows.Forms.ComboBox();
      this.ckBoxFileAssoc = new System.Windows.Forms.CheckBox();
      this.panel8 = new System.Windows.Forms.Panel();
      this.comBoxAaxCopy = new System.Windows.Forms.ComboBox();
      this.btnAaxCopyDir = new System.Windows.Forms.Button();
      this.ckBoxLaunchPlayer = new System.Windows.Forms.CheckBox();
      this.ckBoxLatin1 = new System.Windows.Forms.CheckBox();
      this.comBoxM4B = new System.Windows.Forms.ComboBox();
      this.ckBoxExtraMetaFiles = new System.Windows.Forms.CheckBox();
      this.lblLang = new System.Windows.Forms.Label();
      this.lblUpdate = new System.Windows.Forms.Label();
      this.lblFileAssoc = new System.Windows.Forms.Label();
      this.lblAaxCopy = new System.Windows.Forms.Label();
      this.lblLaunchPlayer = new System.Windows.Forms.Label();
      this.lblLatin1 = new System.Windows.Forms.Label();
      this.lblM4B = new System.Windows.Forms.Label();
      this.lblExtraMetaFiles = new System.Windows.Forms.Label();
      this.comBoxVerAdjChapters = new System.Windows.Forms.ComboBox();
      this.lblVerAdjChapters = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.panel7 = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panel6.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudVeryShortChapter)).BeginInit();
      this.panel5.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudShortChapter)).BeginInit();
      this.panel4.SuspendLayout();
      this.panel8.SuspendLayout();
      this.panel7.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnReset);
      this.panel1.Controls.Add(this.btnOK);
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Name = "panel1";
      // 
      // btnReset
      // 
      resources.ApplyResources(this.btnReset, "btnReset");
      this.btnReset.Name = "btnReset";
      this.toolTip1.SetToolTip(this.btnReset, resources.GetString("btnReset.ToolTip"));
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.lblUsrActCode, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.lblFfmpegLoc, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnUsrActCode, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.btnFfmpegLoc, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.lblCustTitleChars, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.txtBoxCustTitleChars, 1, 5);
      this.tableLayoutPanel1.Controls.Add(this.txtBoxCustPart, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.lblCustPart, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.panel6, 1, 9);
      this.tableLayoutPanel1.Controls.Add(this.label4, 0, 9);
      this.tableLayoutPanel1.Controls.Add(this.label3, 0, 8);
      this.tableLayoutPanel1.Controls.Add(this.panel5, 1, 8);
      this.tableLayoutPanel1.Controls.Add(this.lblFlatFolders, 0, 7);
      this.tableLayoutPanel1.Controls.Add(this.panel4, 1, 7);
      this.tableLayoutPanel1.Controls.Add(this.comBoxNamedChapters, 1, 6);
      this.tableLayoutPanel1.Controls.Add(this.lblNamedChapters, 0, 6);
      this.tableLayoutPanel1.Controls.Add(this.lblRegActCode, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.lblPartName, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.comBoxLang, 1, 18);
      this.tableLayoutPanel1.Controls.Add(this.comBoxUpdate, 1, 17);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxFileAssoc, 1, 16);
      this.tableLayoutPanel1.Controls.Add(this.panel8, 1, 15);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxLaunchPlayer, 1, 14);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxLatin1, 1, 13);
      this.tableLayoutPanel1.Controls.Add(this.comBoxM4B, 1, 12);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxExtraMetaFiles, 1, 11);
      this.tableLayoutPanel1.Controls.Add(this.lblLang, 0, 18);
      this.tableLayoutPanel1.Controls.Add(this.lblUpdate, 0, 17);
      this.tableLayoutPanel1.Controls.Add(this.lblFileAssoc, 0, 16);
      this.tableLayoutPanel1.Controls.Add(this.lblAaxCopy, 0, 15);
      this.tableLayoutPanel1.Controls.Add(this.lblLaunchPlayer, 0, 14);
      this.tableLayoutPanel1.Controls.Add(this.lblLatin1, 0, 13);
      this.tableLayoutPanel1.Controls.Add(this.lblM4B, 0, 12);
      this.tableLayoutPanel1.Controls.Add(this.lblExtraMetaFiles, 0, 11);
      this.tableLayoutPanel1.Controls.Add(this.comBoxVerAdjChapters, 1, 10);
      this.tableLayoutPanel1.Controls.Add(this.lblVerAdjChapters, 0, 10);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // lblUsrActCode
      // 
      resources.ApplyResources(this.lblUsrActCode, "lblUsrActCode");
      this.lblUsrActCode.Name = "lblUsrActCode";
      // 
      // lblFfmpegLoc
      // 
      resources.ApplyResources(this.lblFfmpegLoc, "lblFfmpegLoc");
      this.lblFfmpegLoc.Name = "lblFfmpegLoc";
      // 
      // btnUsrActCode
      // 
      resources.ApplyResources(this.btnUsrActCode, "btnUsrActCode");
      this.btnUsrActCode.Name = "btnUsrActCode";
      this.btnUsrActCode.UseVisualStyleBackColor = true;
      this.btnUsrActCode.Click += new System.EventHandler(this.btnUsrActCode_Click);
      // 
      // btnFfmpegLoc
      // 
      resources.ApplyResources(this.btnFfmpegLoc, "btnFfmpegLoc");
      this.btnFfmpegLoc.Name = "btnFfmpegLoc";
      this.btnFfmpegLoc.UseVisualStyleBackColor = true;
      this.btnFfmpegLoc.Click += new System.EventHandler(this.btnFfmpegLoc_Click);
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.listBoxActCode);
      this.panel2.Controls.Add(this.btnRegActCode);
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Name = "panel2";
      // 
      // listBoxActCode
      // 
      this.listBoxActCode.FormattingEnabled = true;
      resources.ApplyResources(this.listBoxActCode, "listBoxActCode");
      this.listBoxActCode.Name = "listBoxActCode";
      this.listBoxActCode.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      // 
      // btnRegActCode
      // 
      resources.ApplyResources(this.btnRegActCode, "btnRegActCode");
      this.btnRegActCode.Name = "btnRegActCode";
      this.btnRegActCode.UseVisualStyleBackColor = true;
      this.btnRegActCode.Click += new System.EventHandler(this.btnRegActCode_Click);
      // 
      // lblCustTitleChars
      // 
      resources.ApplyResources(this.lblCustTitleChars, "lblCustTitleChars");
      this.lblCustTitleChars.Name = "lblCustTitleChars";
      // 
      // txtBoxCustTitleChars
      // 
      resources.ApplyResources(this.txtBoxCustTitleChars, "txtBoxCustTitleChars");
      this.txtBoxCustTitleChars.Name = "txtBoxCustTitleChars";
      this.toolTip1.SetToolTip(this.txtBoxCustTitleChars, resources.GetString("txtBoxCustTitleChars.ToolTip"));
      this.txtBoxCustTitleChars.TextChanged += new System.EventHandler(this.txtBoxCustTitleChars_TextChanged);
      // 
      // txtBoxCustPart
      // 
      resources.ApplyResources(this.txtBoxCustPart, "txtBoxCustPart");
      this.txtBoxCustPart.Name = "txtBoxCustPart";
      this.toolTip1.SetToolTip(this.txtBoxCustPart, resources.GetString("txtBoxCustPart.ToolTip"));
      this.txtBoxCustPart.Leave += new System.EventHandler(this.txtBoxCustPart_Leave);
      // 
      // lblCustPart
      // 
      resources.ApplyResources(this.lblCustPart, "lblCustPart");
      this.lblCustPart.Name = "lblCustPart";
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.txtBoxPartName);
      this.panel3.Controls.Add(this.comBoxPartName);
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Name = "panel3";
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
      // panel6
      // 
      this.panel6.Controls.Add(this.label2);
      this.panel6.Controls.Add(this.nudVeryShortChapter);
      resources.ApplyResources(this.panel6, "panel6");
      this.panel6.Name = "panel6";
      // 
      // label2
      // 
      resources.ApplyResources(this.label2, "label2");
      this.label2.Name = "label2";
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
      // label4
      // 
      resources.ApplyResources(this.label4, "label4");
      this.label4.Name = "label4";
      // 
      // label3
      // 
      resources.ApplyResources(this.label3, "label3");
      this.label3.Name = "label3";
      // 
      // panel5
      // 
      this.panel5.Controls.Add(this.label1);
      this.panel5.Controls.Add(this.nudShortChapter);
      resources.ApplyResources(this.panel5, "panel5");
      this.panel5.Name = "panel5";
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
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
      // lblFlatFolders
      // 
      resources.ApplyResources(this.lblFlatFolders, "lblFlatFolders");
      this.lblFlatFolders.Name = "lblFlatFolders";
      // 
      // panel4
      // 
      this.panel4.Controls.Add(this.comBoxFlatFolders);
      this.panel4.Controls.Add(this.ckBoxFlatFolders);
      resources.ApplyResources(this.panel4, "panel4");
      this.panel4.Name = "panel4";
      // 
      // comBoxFlatFolders
      // 
      this.comBoxFlatFolders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxFlatFolders.FormattingEnabled = true;
      resources.ApplyResources(this.comBoxFlatFolders, "comBoxFlatFolders");
      this.comBoxFlatFolders.Name = "comBoxFlatFolders";
      // 
      // ckBoxFlatFolders
      // 
      resources.ApplyResources(this.ckBoxFlatFolders, "ckBoxFlatFolders");
      this.ckBoxFlatFolders.Name = "ckBoxFlatFolders";
      this.toolTip1.SetToolTip(this.ckBoxFlatFolders, resources.GetString("ckBoxFlatFolders.ToolTip"));
      this.ckBoxFlatFolders.UseVisualStyleBackColor = true;
      this.ckBoxFlatFolders.CheckedChanged += new System.EventHandler(this.ckBoxFlatFolders_CheckedChanged);
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
      // lblNamedChapters
      // 
      resources.ApplyResources(this.lblNamedChapters, "lblNamedChapters");
      this.lblNamedChapters.Name = "lblNamedChapters";
      this.toolTip1.SetToolTip(this.lblNamedChapters, resources.GetString("lblNamedChapters.ToolTip"));
      // 
      // lblRegActCode
      // 
      resources.ApplyResources(this.lblRegActCode, "lblRegActCode");
      this.lblRegActCode.Name = "lblRegActCode";
      // 
      // lblPartName
      // 
      resources.ApplyResources(this.lblPartName, "lblPartName");
      this.lblPartName.Name = "lblPartName";
      // 
      // comBoxLang
      // 
      resources.ApplyResources(this.comBoxLang, "comBoxLang");
      this.comBoxLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxLang.FormattingEnabled = true;
      this.comBoxLang.Items.AddRange(new object[] {
            resources.GetString("comBoxLang.Items")});
      this.comBoxLang.Name = "comBoxLang";
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
      // 
      // ckBoxFileAssoc
      // 
      resources.ApplyResources(this.ckBoxFileAssoc, "ckBoxFileAssoc");
      this.ckBoxFileAssoc.Name = "ckBoxFileAssoc";
      this.toolTip1.SetToolTip(this.ckBoxFileAssoc, resources.GetString("ckBoxFileAssoc.ToolTip"));
      this.ckBoxFileAssoc.UseVisualStyleBackColor = true;
      // 
      // panel8
      // 
      this.panel8.Controls.Add(this.comBoxAaxCopy);
      this.panel8.Controls.Add(this.btnAaxCopyDir);
      resources.ApplyResources(this.panel8, "panel8");
      this.panel8.Name = "panel8";
      // 
      // comBoxAaxCopy
      // 
      this.comBoxAaxCopy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comBoxAaxCopy.FormattingEnabled = true;
      resources.ApplyResources(this.comBoxAaxCopy, "comBoxAaxCopy");
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
      // ckBoxLaunchPlayer
      // 
      resources.ApplyResources(this.ckBoxLaunchPlayer, "ckBoxLaunchPlayer");
      this.ckBoxLaunchPlayer.Name = "ckBoxLaunchPlayer";
      this.toolTip1.SetToolTip(this.ckBoxLaunchPlayer, resources.GetString("ckBoxLaunchPlayer.ToolTip"));
      this.ckBoxLaunchPlayer.UseVisualStyleBackColor = true;
      // 
      // ckBoxLatin1
      // 
      resources.ApplyResources(this.ckBoxLatin1, "ckBoxLatin1");
      this.ckBoxLatin1.Name = "ckBoxLatin1";
      this.toolTip1.SetToolTip(this.ckBoxLatin1, resources.GetString("ckBoxLatin1.ToolTip"));
      this.ckBoxLatin1.UseVisualStyleBackColor = true;
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
      // ckBoxExtraMetaFiles
      // 
      resources.ApplyResources(this.ckBoxExtraMetaFiles, "ckBoxExtraMetaFiles");
      this.ckBoxExtraMetaFiles.Name = "ckBoxExtraMetaFiles";
      this.toolTip1.SetToolTip(this.ckBoxExtraMetaFiles, resources.GetString("ckBoxExtraMetaFiles.ToolTip"));
      this.ckBoxExtraMetaFiles.UseVisualStyleBackColor = true;
      // 
      // lblLang
      // 
      resources.ApplyResources(this.lblLang, "lblLang");
      this.lblLang.Name = "lblLang";
      // 
      // lblUpdate
      // 
      resources.ApplyResources(this.lblUpdate, "lblUpdate");
      this.lblUpdate.Name = "lblUpdate";
      // 
      // lblFileAssoc
      // 
      resources.ApplyResources(this.lblFileAssoc, "lblFileAssoc");
      this.lblFileAssoc.Name = "lblFileAssoc";
      // 
      // lblAaxCopy
      // 
      resources.ApplyResources(this.lblAaxCopy, "lblAaxCopy");
      this.lblAaxCopy.Name = "lblAaxCopy";
      // 
      // lblLaunchPlayer
      // 
      resources.ApplyResources(this.lblLaunchPlayer, "lblLaunchPlayer");
      this.lblLaunchPlayer.Name = "lblLaunchPlayer";
      // 
      // lblLatin1
      // 
      resources.ApplyResources(this.lblLatin1, "lblLatin1");
      this.lblLatin1.Name = "lblLatin1";
      // 
      // lblM4B
      // 
      resources.ApplyResources(this.lblM4B, "lblM4B");
      this.lblM4B.Name = "lblM4B";
      // 
      // lblExtraMetaFiles
      // 
      resources.ApplyResources(this.lblExtraMetaFiles, "lblExtraMetaFiles");
      this.lblExtraMetaFiles.Name = "lblExtraMetaFiles";
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
      this.comBoxVerAdjChapters.SelectedIndexChanged += new System.EventHandler(this.comBoxPartName_SelectedIndexChanged);
      // 
      // lblVerAdjChapters
      // 
      resources.ApplyResources(this.lblVerAdjChapters, "lblVerAdjChapters");
      this.lblVerAdjChapters.Name = "lblVerAdjChapters";
      // 
      // panel7
      // 
      this.panel7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panel7.Controls.Add(this.tableLayoutPanel1);
      resources.ApplyResources(this.panel7, "panel7");
      this.panel7.Name = "panel7";
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.btnOK;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOK;
      this.Controls.Add(this.panel7);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SettingsForm";
      this.panel1.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.panel6.ResumeLayout(false);
      this.panel6.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudVeryShortChapter)).EndInit();
      this.panel5.ResumeLayout(false);
      this.panel5.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudShortChapter)).EndInit();
      this.panel4.ResumeLayout(false);
      this.panel8.ResumeLayout(false);
      this.panel7.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
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
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.ListBox listBoxActCode;
    private System.Windows.Forms.Label lblCustTitleChars;
    private System.Windows.Forms.TextBox txtBoxCustTitleChars;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.TextBox txtBoxPartName;
    private System.Windows.Forms.ComboBox comBoxPartName;
    private System.Windows.Forms.Label lblUpdate;
    private System.Windows.Forms.ComboBox comBoxUpdate;
    private System.Windows.Forms.Panel panel4;
    private System.Windows.Forms.ComboBox comBoxFlatFolders;
    private System.Windows.Forms.CheckBox ckBoxFlatFolders;
    private System.Windows.Forms.CheckBox ckBoxExtraMetaFiles;
    private System.Windows.Forms.Label lblExtraMetaFiles;
    private System.Windows.Forms.Label lblFlatFolders;
    private System.Windows.Forms.Panel panel5;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown nudShortChapter;
    private System.Windows.Forms.Panel panel6;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown nudVeryShortChapter;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox ckBoxLatin1;
    private System.Windows.Forms.Label lblLatin1;
    private System.Windows.Forms.Panel panel7;
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
    private System.Windows.Forms.Panel panel8;
    private System.Windows.Forms.ComboBox comBoxAaxCopy;
    private System.Windows.Forms.Button btnAaxCopyDir;
    private System.Windows.Forms.ComboBox comBoxVerAdjChapters;
    private System.Windows.Forms.Label lblVerAdjChapters;
  }
}