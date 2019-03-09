namespace audiamus.aaxconv {
  partial class AboutForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
      this.lblIcon = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.linkLabelHomepage = new System.Windows.Forms.LinkLabel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.lblProduct = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      this.lblCopyright = new System.Windows.Forms.Label();
      this.lblHomepageHint = new System.Windows.Forms.Label();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblIcon
      // 
      resources.ApplyResources(this.lblIcon, "lblIcon");
      this.lblIcon.Image = global::audiamus.aaxconv.Properties.Resources.Audio48;
      this.lblIcon.Name = "lblIcon";
      // 
      // button1
      // 
      resources.ApplyResources(this.button1, "button1");
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.Name = "button1";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // linkLabelHomepage
      // 
      resources.ApplyResources(this.linkLabelHomepage, "linkLabelHomepage");
      this.linkLabelHomepage.Name = "linkLabelHomepage";
      this.linkLabelHomepage.TabStop = true;
      this.linkLabelHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHomepage_LinkClicked);
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.lblProduct, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblVersion, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.linkLabelHomepage, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.lblCopyright, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.lblHomepageHint, 0, 3);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // lblProduct
      // 
      resources.ApplyResources(this.lblProduct, "lblProduct");
      this.lblProduct.Name = "lblProduct";
      // 
      // lblVersion
      // 
      resources.ApplyResources(this.lblVersion, "lblVersion");
      this.lblVersion.Name = "lblVersion";
      // 
      // lblCopyright
      // 
      resources.ApplyResources(this.lblCopyright, "lblCopyright");
      this.lblCopyright.Name = "lblCopyright";
      // 
      // lblHomepageHint
      // 
      resources.ApplyResources(this.lblHomepageHint, "lblHomepageHint");
      this.lblHomepageHint.Name = "lblHomepageHint";
      // 
      // AboutForm
      // 
      this.AcceptButton = this.button1;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.button1;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.lblIcon);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutForm";
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblIcon;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.LinkLabel linkLabelHomepage;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label lblProduct;
    private System.Windows.Forms.Label lblVersion;
    private System.Windows.Forms.Label lblCopyright;
    private System.Windows.Forms.Label lblHomepageHint;
  }
}