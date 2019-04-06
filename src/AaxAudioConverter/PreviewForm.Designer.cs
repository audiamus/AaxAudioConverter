namespace audiamus.aaxconv {
  partial class PreviewForm {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblSts = new System.Windows.Forms.Label();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.table = new System.Windows.Forms.TableLayoutPanel();
      this.lblSource = new System.Windows.Forms.Label();
      this.lblTag = new System.Windows.Forms.Label();
      this.comBoxGenreTag = new System.Windows.Forms.ComboBox();
      this.txtBoxAuthorFile = new System.Windows.Forms.TextBox();
      this.numUpDnYearTag = new System.Windows.Forms.NumericUpDown();
      this.txtBoxAuthorTag = new System.Windows.Forms.TextBox();
      this.txtBoxTitleSource = new System.Windows.Forms.TextBox();
      this.lblFile = new System.Windows.Forms.Label();
      this.txtBoxAuthorSource = new System.Windows.Forms.TextBox();
      this.txtBoxTitleTag = new System.Windows.Forms.TextBox();
      this.lblAuthor = new System.Windows.Forms.Label();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblYear = new System.Windows.Forms.Label();
      this.lblGenre = new System.Windows.Forms.Label();
      this.txtBoxYearSource = new System.Windows.Forms.TextBox();
      this.btnGenres = new System.Windows.Forms.Button();
      this.txtBoxGenreSource = new System.Windows.Forms.TextBox();
      this.txtBoxTitleFile = new System.Windows.Forms.TextBox();
      this.panel1.SuspendLayout();
      this.table.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numUpDnYearTag)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.lblSts);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOK);
      this.panel1.Name = "panel1";
      // 
      // lblSts
      // 
      resources.ApplyResources(this.lblSts, "lblSts");
      this.lblSts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblSts.Name = "lblSts";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnOK
      // 
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.Name = "btnOK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // table
      // 
      resources.ApplyResources(this.table, "table");
      this.table.Controls.Add(this.lblSource, 0, 1);
      this.table.Controls.Add(this.lblTag, 0, 2);
      this.table.Controls.Add(this.comBoxGenreTag, 4, 2);
      this.table.Controls.Add(this.txtBoxAuthorFile, 1, 3);
      this.table.Controls.Add(this.numUpDnYearTag, 3, 2);
      this.table.Controls.Add(this.txtBoxAuthorTag, 1, 2);
      this.table.Controls.Add(this.txtBoxTitleSource, 2, 1);
      this.table.Controls.Add(this.lblFile, 0, 3);
      this.table.Controls.Add(this.txtBoxAuthorSource, 1, 1);
      this.table.Controls.Add(this.txtBoxTitleTag, 2, 2);
      this.table.Controls.Add(this.lblAuthor, 1, 0);
      this.table.Controls.Add(this.lblTitle, 2, 0);
      this.table.Controls.Add(this.lblYear, 3, 0);
      this.table.Controls.Add(this.lblGenre, 4, 0);
      this.table.Controls.Add(this.txtBoxYearSource, 3, 1);
      this.table.Controls.Add(this.btnGenres, 4, 3);
      this.table.Controls.Add(this.txtBoxGenreSource, 4, 1);
      this.table.Controls.Add(this.txtBoxTitleFile, 2, 3);
      this.table.Name = "table";
      // 
      // lblSource
      // 
      resources.ApplyResources(this.lblSource, "lblSource");
      this.lblSource.Name = "lblSource";
      // 
      // lblTag
      // 
      resources.ApplyResources(this.lblTag, "lblTag");
      this.lblTag.Name = "lblTag";
      // 
      // comBoxGenreTag
      // 
      resources.ApplyResources(this.comBoxGenreTag, "comBoxGenreTag");
      this.comBoxGenreTag.FormattingEnabled = true;
      this.comBoxGenreTag.Name = "comBoxGenreTag";
      this.comBoxGenreTag.Sorted = true;
      this.comBoxGenreTag.TextChanged += new System.EventHandler(this.txtBox_TextChanged);
      this.comBoxGenreTag.Leave += new System.EventHandler(this.txtBox_Leave);
      // 
      // txtBoxAuthorFile
      // 
      resources.ApplyResources(this.txtBoxAuthorFile, "txtBoxAuthorFile");
      this.txtBoxAuthorFile.Name = "txtBoxAuthorFile";
      this.txtBoxAuthorFile.TextChanged += new System.EventHandler(this.txtBox_TextChanged);
      this.txtBoxAuthorFile.Leave += new System.EventHandler(this.txtBox_Leave);
      // 
      // numUpDnYearTag
      // 
      resources.ApplyResources(this.numUpDnYearTag, "numUpDnYearTag");
      this.numUpDnYearTag.Maximum = new decimal(new int[] {
            2200,
            0,
            0,
            0});
      this.numUpDnYearTag.Name = "numUpDnYearTag";
      this.numUpDnYearTag.ValueChanged += new System.EventHandler(this.txtBox_TextChanged);
      this.numUpDnYearTag.Leave += new System.EventHandler(this.txtBox_Leave);
      this.numUpDnYearTag.Validating += new System.ComponentModel.CancelEventHandler(this.numUpDnYearTag_Validating);
      // 
      // txtBoxAuthorTag
      // 
      resources.ApplyResources(this.txtBoxAuthorTag, "txtBoxAuthorTag");
      this.txtBoxAuthorTag.Name = "txtBoxAuthorTag";
      this.txtBoxAuthorTag.TextChanged += new System.EventHandler(this.txtBox_TextChanged);
      this.txtBoxAuthorTag.Leave += new System.EventHandler(this.txtBox_Leave);
      // 
      // txtBoxTitleSource
      // 
      resources.ApplyResources(this.txtBoxTitleSource, "txtBoxTitleSource");
      this.txtBoxTitleSource.Name = "txtBoxTitleSource";
      this.txtBoxTitleSource.ReadOnly = true;
      // 
      // lblFile
      // 
      resources.ApplyResources(this.lblFile, "lblFile");
      this.lblFile.Name = "lblFile";
      // 
      // txtBoxAuthorSource
      // 
      resources.ApplyResources(this.txtBoxAuthorSource, "txtBoxAuthorSource");
      this.txtBoxAuthorSource.Name = "txtBoxAuthorSource";
      this.txtBoxAuthorSource.ReadOnly = true;
      // 
      // txtBoxTitleTag
      // 
      resources.ApplyResources(this.txtBoxTitleTag, "txtBoxTitleTag");
      this.txtBoxTitleTag.Name = "txtBoxTitleTag";
      this.txtBoxTitleTag.TextChanged += new System.EventHandler(this.txtBox_TextChanged);
      this.txtBoxTitleTag.Leave += new System.EventHandler(this.txtBox_Leave);
      // 
      // lblAuthor
      // 
      resources.ApplyResources(this.lblAuthor, "lblAuthor");
      this.lblAuthor.Name = "lblAuthor";
      // 
      // lblTitle
      // 
      resources.ApplyResources(this.lblTitle, "lblTitle");
      this.lblTitle.Name = "lblTitle";
      // 
      // lblYear
      // 
      resources.ApplyResources(this.lblYear, "lblYear");
      this.lblYear.Name = "lblYear";
      // 
      // lblGenre
      // 
      resources.ApplyResources(this.lblGenre, "lblGenre");
      this.lblGenre.Name = "lblGenre";
      // 
      // txtBoxYearSource
      // 
      resources.ApplyResources(this.txtBoxYearSource, "txtBoxYearSource");
      this.txtBoxYearSource.Name = "txtBoxYearSource";
      this.txtBoxYearSource.ReadOnly = true;
      // 
      // btnGenres
      // 
      resources.ApplyResources(this.btnGenres, "btnGenres");
      this.btnGenres.Name = "btnGenres";
      this.btnGenres.UseVisualStyleBackColor = true;
      this.btnGenres.Click += new System.EventHandler(this.btnGenres_Click);
      // 
      // txtBoxGenreSource
      // 
      resources.ApplyResources(this.txtBoxGenreSource, "txtBoxGenreSource");
      this.txtBoxGenreSource.Name = "txtBoxGenreSource";
      this.txtBoxGenreSource.ReadOnly = true;
      // 
      // txtBoxTitleFile
      // 
      resources.ApplyResources(this.txtBoxTitleFile, "txtBoxTitleFile");
      this.txtBoxTitleFile.Name = "txtBoxTitleFile";
      this.txtBoxTitleFile.TextChanged += new System.EventHandler(this.txtBox_TextChanged);
      this.txtBoxTitleFile.Leave += new System.EventHandler(this.txtBox_Leave);
      // 
      // PreviewForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.table);
      this.Controls.Add(this.panel1);
      this.Name = "PreviewForm";
      this.ShowIcon = false;
      this.panel1.ResumeLayout(false);
      this.table.ResumeLayout(false);
      this.table.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numUpDnYearTag)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.TableLayoutPanel table;
    private System.Windows.Forms.Label lblSource;
    private System.Windows.Forms.Label lblTag;
    private System.Windows.Forms.Label lblFile;
    private System.Windows.Forms.Label lblAuthor;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblYear;
    private System.Windows.Forms.Label lblGenre;
    private System.Windows.Forms.TextBox txtBoxAuthorSource;
    private System.Windows.Forms.TextBox txtBoxTitleSource;
    private System.Windows.Forms.TextBox txtBoxYearSource;
    private System.Windows.Forms.TextBox txtBoxGenreSource;
    private System.Windows.Forms.TextBox txtBoxAuthorTag;
    private System.Windows.Forms.TextBox txtBoxTitleTag;
    private System.Windows.Forms.TextBox txtBoxAuthorFile;
    private System.Windows.Forms.TextBox txtBoxTitleFile;
    private System.Windows.Forms.ComboBox comBoxGenreTag;
    private System.Windows.Forms.Button btnGenres;
    private System.Windows.Forms.NumericUpDown numUpDnYearTag;
    private System.Windows.Forms.Label lblSts;
  }
}