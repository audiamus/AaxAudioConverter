namespace audiamus.aaxconv {
  partial class FFmpegLocationForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FFmpegLocationForm));
      this.textBoxLocation = new System.Windows.Forms.TextBox();
      this.btnLocate = new System.Windows.Forms.Button();
      this.labelCaption = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // textBoxLocation
      // 
      resources.ApplyResources(this.textBoxLocation, "textBoxLocation");
      this.textBoxLocation.Name = "textBoxLocation";
      this.textBoxLocation.ReadOnly = true;
      // 
      // btnLocate
      // 
      resources.ApplyResources(this.btnLocate, "btnLocate");
      this.btnLocate.Name = "btnLocate";
      this.btnLocate.UseVisualStyleBackColor = true;
      this.btnLocate.Click += new System.EventHandler(this.btnLocate_Click);
      // 
      // labelCaption
      // 
      resources.ApplyResources(this.labelCaption, "labelCaption");
      this.labelCaption.Name = "labelCaption";
      // 
      // btnOK
      // 
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // FFmpegLocationForm
      // 
      this.AcceptButton = this.btnLocate;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.labelCaption);
      this.Controls.Add(this.btnLocate);
      this.Controls.Add(this.textBoxLocation);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FFmpegLocationForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBoxLocation;
    private System.Windows.Forms.Button btnLocate;
    private System.Windows.Forms.Label labelCaption;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
  }
}