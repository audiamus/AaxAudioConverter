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
      this.lblRegActCode = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.listBoxActCode = new System.Windows.Forms.ListBox();
      this.btnRegActCode = new System.Windows.Forms.Button();
      this.cmBoxLang = new System.Windows.Forms.ComboBox();
      this.lblLang = new System.Windows.Forms.Label();
      this.ckBoxFileAssoc = new System.Windows.Forms.CheckBox();
      this.lblFileAssoc = new System.Windows.Forms.Label();
      this.lblCustTitleChars = new System.Windows.Forms.Label();
      this.txtBoxCustTitleChars = new System.Windows.Forms.TextBox();
      this.txtBoxCustPart = new System.Windows.Forms.TextBox();
      this.lblCustPart = new System.Windows.Forms.Label();
      this.panel3 = new System.Windows.Forms.Panel();
      this.txtBoxPartName = new System.Windows.Forms.TextBox();
      this.cmBoxPartName = new System.Windows.Forms.ComboBox();
      this.lblPartName = new System.Windows.Forms.Label();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.panel1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
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
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
      this.tableLayoutPanel1.Controls.Add(this.lblRegActCode, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.cmBoxLang, 1, 7);
      this.tableLayoutPanel1.Controls.Add(this.lblLang, 0, 7);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxFileAssoc, 1, 6);
      this.tableLayoutPanel1.Controls.Add(this.lblFileAssoc, 0, 6);
      this.tableLayoutPanel1.Controls.Add(this.lblCustTitleChars, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.txtBoxCustTitleChars, 1, 5);
      this.tableLayoutPanel1.Controls.Add(this.txtBoxCustPart, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.lblCustPart, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.lblPartName, 0, 3);
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
      // lblRegActCode
      // 
      resources.ApplyResources(this.lblRegActCode, "lblRegActCode");
      this.lblRegActCode.Name = "lblRegActCode";
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
      // cmBoxLang
      // 
      resources.ApplyResources(this.cmBoxLang, "cmBoxLang");
      this.cmBoxLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmBoxLang.FormattingEnabled = true;
      this.cmBoxLang.Items.AddRange(new object[] {
            resources.GetString("cmBoxLang.Items")});
      this.cmBoxLang.Name = "cmBoxLang";
      // 
      // lblLang
      // 
      resources.ApplyResources(this.lblLang, "lblLang");
      this.lblLang.Name = "lblLang";
      // 
      // ckBoxFileAssoc
      // 
      resources.ApplyResources(this.ckBoxFileAssoc, "ckBoxFileAssoc");
      this.ckBoxFileAssoc.Name = "ckBoxFileAssoc";
      this.ckBoxFileAssoc.UseVisualStyleBackColor = true;
      // 
      // lblFileAssoc
      // 
      resources.ApplyResources(this.lblFileAssoc, "lblFileAssoc");
      this.lblFileAssoc.Name = "lblFileAssoc";
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
      this.txtBoxCustTitleChars.Leave += new System.EventHandler(this.txtBoxCustTitleChars_Leave);
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
      this.panel3.Controls.Add(this.cmBoxPartName);
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Name = "panel3";
      // 
      // txtBoxPartName
      // 
      resources.ApplyResources(this.txtBoxPartName, "txtBoxPartName");
      this.txtBoxPartName.Name = "txtBoxPartName";
      this.toolTip1.SetToolTip(this.txtBoxPartName, resources.GetString("txtBoxPartName.ToolTip"));
      // 
      // cmBoxPartName
      // 
      resources.ApplyResources(this.cmBoxPartName, "cmBoxPartName");
      this.cmBoxPartName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmBoxPartName.FormattingEnabled = true;
      this.cmBoxPartName.Name = "cmBoxPartName";
      this.toolTip1.SetToolTip(this.cmBoxPartName, resources.GetString("cmBoxPartName.ToolTip"));
      this.cmBoxPartName.SelectedIndexChanged += new System.EventHandler(this.cmBoxPartName_SelectedIndexChanged);
      // 
      // lblPartName
      // 
      resources.ApplyResources(this.lblPartName, "lblPartName");
      this.lblPartName.Name = "lblPartName";
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.btnOK;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOK;
      this.Controls.Add(this.tableLayoutPanel1);
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
    private System.Windows.Forms.Label lblRegActCode;
    private System.Windows.Forms.Label lblFfmpegLoc;
    private System.Windows.Forms.Button btnRegActCode;
    private System.Windows.Forms.Button btnUsrActCode;
    private System.Windows.Forms.Button btnFfmpegLoc;
    private System.Windows.Forms.TextBox txtBoxCustPart;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.Label lblLang;
    private System.Windows.Forms.ComboBox cmBoxLang;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.ListBox listBoxActCode;
    private System.Windows.Forms.Label lblCustTitleChars;
    private System.Windows.Forms.TextBox txtBoxCustTitleChars;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.TextBox txtBoxPartName;
    private System.Windows.Forms.ComboBox cmBoxPartName;
    private System.Windows.Forms.Label lblPartName;
  }
}