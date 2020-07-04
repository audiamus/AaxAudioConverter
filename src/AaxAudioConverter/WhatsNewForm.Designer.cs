namespace audiamus.aaxconv {
  partial class WhatsNewForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WhatsNewForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnOK = new System.Windows.Forms.Button();
      this.lblPrev = new System.Windows.Forms.Label();
      this.webBrowser = new System.Windows.Forms.WebBrowser();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Controls.Add(this.lblPrev);
      this.panel1.Name = "panel1";
      // 
      // btnOK
      // 
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // lblPrev
      // 
      resources.ApplyResources(this.lblPrev, "lblPrev");
      this.lblPrev.Name = "lblPrev";
      // 
      // webBrowser
      // 
      resources.ApplyResources(this.webBrowser, "webBrowser");
      this.webBrowser.AllowWebBrowserDrop = false;
      this.webBrowser.Name = "webBrowser";
      this.webBrowser.Url = new System.Uri("", System.UriKind.Relative);
      // 
      // WhatsNewForm
      // 
      this.AcceptButton = this.btnOK;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOK;
      this.ControlBox = false;
      this.Controls.Add(this.webBrowser);
      this.Controls.Add(this.panel1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "WhatsNewForm";
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Label lblPrev;
    private System.Windows.Forms.WebBrowser webBrowser;
  }
}