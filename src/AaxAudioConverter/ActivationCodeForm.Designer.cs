namespace audiamus.aaxconv {
  partial class ActivationCodeForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivationCodeForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnDummy = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.lblInfo = new System.Windows.Forms.Label();
      this.lblFormat = new System.Windows.Forms.Label();
      this.linkLblBLC = new System.Windows.Forms.LinkLabel();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnDummy);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Name = "panel1";
      // 
      // btnDummy
      // 
      resources.ApplyResources(this.btnDummy, "btnDummy");
      this.btnDummy.Name = "btnDummy";
      this.toolTip1.SetToolTip(this.btnDummy, resources.GetString("btnDummy.ToolTip"));
      this.btnDummy.UseVisualStyleBackColor = true;
      this.btnDummy.Click += new System.EventHandler(this.btnDummy_Click);
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // textBox1
      // 
      resources.ApplyResources(this.textBox1, "textBox1");
      this.textBox1.Name = "textBox1";
      this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
      // 
      // lblInfo
      // 
      resources.ApplyResources(this.lblInfo, "lblInfo");
      this.lblInfo.Name = "lblInfo";
      // 
      // lblFormat
      // 
      resources.ApplyResources(this.lblFormat, "lblFormat");
      this.lblFormat.Name = "lblFormat";
      // 
      // linkLblBLC
      // 
      resources.ApplyResources(this.linkLblBLC, "linkLblBLC");
      this.linkLblBLC.Name = "linkLblBLC";
      this.linkLblBLC.TabStop = true;
      this.linkLblBLC.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblBLC_LinkClicked);
      // 
      // ActivationCodeForm
      // 
      this.AcceptButton = this.btnCancel;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.linkLblBLC);
      this.Controls.Add(this.lblFormat);
      this.Controls.Add(this.lblInfo);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ActivationCodeForm";
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label lblInfo;
    private System.Windows.Forms.Label lblFormat;
    private System.Windows.Forms.Button btnDummy;
    private System.Windows.Forms.LinkLabel linkLblBLC;
    private System.Windows.Forms.ToolTip toolTip1;
  }
}