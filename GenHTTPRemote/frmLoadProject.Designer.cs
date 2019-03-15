namespace GenHTTP.Remoting {
  partial class frmLoadProject {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent() {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoadProject));
      this.lblFile = new System.Windows.Forms.Label();
      this.txtFile = new System.Windows.Forms.TextBox();
      this.lblType = new System.Windows.Forms.Label();
      this.txtType = new System.Windows.Forms.TextBox();
      this.btnLoad = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblFile
      // 
      this.lblFile.AutoSize = true;
      this.lblFile.Location = new System.Drawing.Point(12, 9);
      this.lblFile.Name = "lblFile";
      this.lblFile.Size = new System.Drawing.Size(36, 13);
      this.lblFile.TabIndex = 0;
      this.lblFile.Text = "lblFile:";
      // 
      // txtFile
      // 
      this.txtFile.Location = new System.Drawing.Point(75, 6);
      this.txtFile.Name = "txtFile";
      this.txtFile.Size = new System.Drawing.Size(244, 20);
      this.txtFile.TabIndex = 1;
      // 
      // lblType
      // 
      this.lblType.AutoSize = true;
      this.lblType.Location = new System.Drawing.Point(12, 34);
      this.lblType.Name = "lblType";
      this.lblType.Size = new System.Drawing.Size(44, 13);
      this.lblType.TabIndex = 2;
      this.lblType.Text = "lblType:";
      // 
      // txtType
      // 
      this.txtType.Location = new System.Drawing.Point(75, 31);
      this.txtType.Name = "txtType";
      this.txtType.Size = new System.Drawing.Size(244, 20);
      this.txtType.TabIndex = 3;
      // 
      // btnLoad
      // 
      this.btnLoad.Location = new System.Drawing.Point(244, 57);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(75, 23);
      this.btnLoad.TabIndex = 4;
      this.btnLoad.Text = "btnLoad";
      this.btnLoad.UseVisualStyleBackColor = true;
      this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
      // 
      // frmLoadProject
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(331, 88);
      this.Controls.Add(this.btnLoad);
      this.Controls.Add(this.txtType);
      this.Controls.Add(this.lblType);
      this.Controls.Add(this.txtFile);
      this.Controls.Add(this.lblFile);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmLoadProject";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "frmLoadProject";
      this.Load += new System.EventHandler(this.frmLoadProject_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblFile;
    private System.Windows.Forms.TextBox txtFile;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.TextBox txtType;
    private System.Windows.Forms.Button btnLoad;
  }
}