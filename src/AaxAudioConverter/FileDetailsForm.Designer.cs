namespace audiamus.aaxconv {
  partial class FileDetailsForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileDetailsForm));
      this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
      this.splitContainerInner = new System.Windows.Forms.SplitContainer();
      this.richTextBoxMeta = new System.Windows.Forms.RichTextBox();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.richTextBoxAbstract = new System.Windows.Forms.RichTextBox();
      this.textBoxTitle = new System.Windows.Forms.TextBox();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).BeginInit();
      this.splitContainerOuter.Panel1.SuspendLayout();
      this.splitContainerOuter.Panel2.SuspendLayout();
      this.splitContainerOuter.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerInner)).BeginInit();
      this.splitContainerInner.Panel1.SuspendLayout();
      this.splitContainerInner.Panel2.SuspendLayout();
      this.splitContainerInner.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // splitContainerOuter
      // 
      resources.ApplyResources(this.splitContainerOuter, "splitContainerOuter");
      this.splitContainerOuter.Name = "splitContainerOuter";
      // 
      // splitContainerOuter.Panel1
      // 
      resources.ApplyResources(this.splitContainerOuter.Panel1, "splitContainerOuter.Panel1");
      this.splitContainerOuter.Panel1.Controls.Add(this.splitContainerInner);
      // 
      // splitContainerOuter.Panel2
      // 
      resources.ApplyResources(this.splitContainerOuter.Panel2, "splitContainerOuter.Panel2");
      this.splitContainerOuter.Panel2.Controls.Add(this.richTextBoxAbstract);
      // 
      // splitContainerInner
      // 
      resources.ApplyResources(this.splitContainerInner, "splitContainerInner");
      this.splitContainerInner.Name = "splitContainerInner";
      // 
      // splitContainerInner.Panel1
      // 
      resources.ApplyResources(this.splitContainerInner.Panel1, "splitContainerInner.Panel1");
      this.splitContainerInner.Panel1.Controls.Add(this.richTextBoxMeta);
      // 
      // splitContainerInner.Panel2
      // 
      resources.ApplyResources(this.splitContainerInner.Panel2, "splitContainerInner.Panel2");
      this.splitContainerInner.Panel2.Controls.Add(this.pictureBox);
      // 
      // richTextBoxMeta
      // 
      resources.ApplyResources(this.richTextBoxMeta, "richTextBoxMeta");
      this.richTextBoxMeta.Name = "richTextBoxMeta";
      this.richTextBoxMeta.ReadOnly = true;
      // 
      // pictureBox
      // 
      resources.ApplyResources(this.pictureBox, "pictureBox");
      this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.TabStop = false;
      // 
      // richTextBoxAbstract
      // 
      resources.ApplyResources(this.richTextBoxAbstract, "richTextBoxAbstract");
      this.richTextBoxAbstract.Name = "richTextBoxAbstract";
      this.richTextBoxAbstract.ReadOnly = true;
      // 
      // textBoxTitle
      // 
      resources.ApplyResources(this.textBoxTitle, "textBoxTitle");
      this.textBoxTitle.Name = "textBoxTitle";
      this.textBoxTitle.ReadOnly = true;
      // 
      // FileDetailsForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainerOuter);
      this.Controls.Add(this.textBoxTitle);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.KeyPreview = true;
      this.Name = "FileDetailsForm";
      this.splitContainerOuter.Panel1.ResumeLayout(false);
      this.splitContainerOuter.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).EndInit();
      this.splitContainerOuter.ResumeLayout(false);
      this.splitContainerInner.Panel1.ResumeLayout(false);
      this.splitContainerInner.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainerInner)).EndInit();
      this.splitContainerInner.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RichTextBox richTextBoxMeta;
    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.SplitContainer splitContainerOuter;
    private System.Windows.Forms.SplitContainer splitContainerInner;
    private System.Windows.Forms.TextBox textBoxTitle;
    private System.Windows.Forms.RichTextBox richTextBoxAbstract;
  }
}