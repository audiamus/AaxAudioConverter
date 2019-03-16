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
      this.lblCustPart = new System.Windows.Forms.Label();
      this.lblUsrActCode = new System.Windows.Forms.Label();
      this.lblFfmpegLoc = new System.Windows.Forms.Label();
      this.btnUsrActCode = new System.Windows.Forms.Button();
      this.btnFfmpegLoc = new System.Windows.Forms.Button();
      this.txtBoxCustPart = new System.Windows.Forms.TextBox();
      this.lblRegActCode = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.listBoxActCode = new System.Windows.Forms.ListBox();
      this.btnRegActCode = new System.Windows.Forms.Button();
      this.lblLang = new System.Windows.Forms.Label();
      this.cmBoxLang = new System.Windows.Forms.ComboBox();
      this.lblFileAssoc = new System.Windows.Forms.Label();
      this.ckBoxFileAssoc = new System.Windows.Forms.CheckBox();
      this.lblCustTitleChars = new System.Windows.Forms.Label();
      this.txtBoxCustTitleChars = new System.Windows.Forms.TextBox();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.panel1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnReset);
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Name = "panel1";
      this.toolTip1.SetToolTip(this.panel1, resources.GetString("panel1.ToolTip"));
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
      this.toolTip1.SetToolTip(this.btnOK, resources.GetString("btnOK.ToolTip"));
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.lblCustPart, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.lblUsrActCode, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.lblFfmpegLoc, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnUsrActCode, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.btnFfmpegLoc, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.txtBoxCustPart, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.lblRegActCode, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.lblLang, 0, 6);
      this.tableLayoutPanel1.Controls.Add(this.cmBoxLang, 1, 6);
      this.tableLayoutPanel1.Controls.Add(this.lblFileAssoc, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.ckBoxFileAssoc, 1, 5);
      this.tableLayoutPanel1.Controls.Add(this.lblCustTitleChars, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.txtBoxCustTitleChars, 1, 4);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.toolTip1.SetToolTip(this.tableLayoutPanel1, resources.GetString("tableLayoutPanel1.ToolTip"));
      // 
      // lblCustPart
      // 
      resources.ApplyResources(this.lblCustPart, "lblCustPart");
      this.lblCustPart.Name = "lblCustPart";
      this.toolTip1.SetToolTip(this.lblCustPart, resources.GetString("lblCustPart.ToolTip"));
      // 
      // lblUsrActCode
      // 
      resources.ApplyResources(this.lblUsrActCode, "lblUsrActCode");
      this.lblUsrActCode.Name = "lblUsrActCode";
      this.toolTip1.SetToolTip(this.lblUsrActCode, resources.GetString("lblUsrActCode.ToolTip"));
      // 
      // lblFfmpegLoc
      // 
      resources.ApplyResources(this.lblFfmpegLoc, "lblFfmpegLoc");
      this.lblFfmpegLoc.Name = "lblFfmpegLoc";
      this.toolTip1.SetToolTip(this.lblFfmpegLoc, resources.GetString("lblFfmpegLoc.ToolTip"));
      // 
      // btnUsrActCode
      // 
      resources.ApplyResources(this.btnUsrActCode, "btnUsrActCode");
      this.btnUsrActCode.Name = "btnUsrActCode";
      this.toolTip1.SetToolTip(this.btnUsrActCode, resources.GetString("btnUsrActCode.ToolTip"));
      this.btnUsrActCode.UseVisualStyleBackColor = true;
      this.btnUsrActCode.Click += new System.EventHandler(this.btnUsrActCode_Click);
      // 
      // btnFfmpegLoc
      // 
      resources.ApplyResources(this.btnFfmpegLoc, "btnFfmpegLoc");
      this.btnFfmpegLoc.Name = "btnFfmpegLoc";
      this.toolTip1.SetToolTip(this.btnFfmpegLoc, resources.GetString("btnFfmpegLoc.ToolTip"));
      this.btnFfmpegLoc.UseVisualStyleBackColor = true;
      this.btnFfmpegLoc.Click += new System.EventHandler(this.btnFfmpegLoc_Click);
      // 
      // txtBoxCustPart
      // 
      resources.ApplyResources(this.txtBoxCustPart, "txtBoxCustPart");
      this.txtBoxCustPart.Name = "txtBoxCustPart";
      this.toolTip1.SetToolTip(this.txtBoxCustPart, resources.GetString("txtBoxCustPart.ToolTip"));
      this.txtBoxCustPart.Leave += new System.EventHandler(this.txtBoxCustPart_Leave);
      // 
      // lblRegActCode
      // 
      resources.ApplyResources(this.lblRegActCode, "lblRegActCode");
      this.lblRegActCode.Name = "lblRegActCode";
      this.toolTip1.SetToolTip(this.lblRegActCode, resources.GetString("lblRegActCode.ToolTip"));
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.listBoxActCode);
      this.panel2.Controls.Add(this.btnRegActCode);
      this.panel2.Name = "panel2";
      this.toolTip1.SetToolTip(this.panel2, resources.GetString("panel2.ToolTip"));
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
      // lblLang
      // 
      resources.ApplyResources(this.lblLang, "lblLang");
      this.lblLang.Name = "lblLang";
      this.toolTip1.SetToolTip(this.lblLang, resources.GetString("lblLang.ToolTip"));
      // 
      // cmBoxLang
      // 
      resources.ApplyResources(this.cmBoxLang, "cmBoxLang");
      this.cmBoxLang.FormattingEnabled = true;
      this.cmBoxLang.Items.AddRange(new object[] {
            resources.GetString("cmBoxLang.Items")});
      this.cmBoxLang.Name = "cmBoxLang";
      this.toolTip1.SetToolTip(this.cmBoxLang, resources.GetString("cmBoxLang.ToolTip"));
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
      this.txtBoxCustTitleChars.Leave += new System.EventHandler(this.txtBoxCustTitleChars_Leave);
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
      this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
      this.panel1.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.panel2.ResumeLayout(false);
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
  }
}