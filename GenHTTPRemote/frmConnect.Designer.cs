namespace GenHTTP.Remoting {
  partial class frmConnect {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConnect));
      this.grpConnect = new System.Windows.Forms.GroupBox();
      this.lblServer = new System.Windows.Forms.Label();
      this.btnConnect = new System.Windows.Forms.Button();
      this.cboAddress = new System.Windows.Forms.ComboBox();
      this.lblInstance = new System.Windows.Forms.Label();
      this.cboInstance = new System.Windows.Forms.ComboBox();
      this.btnManage = new System.Windows.Forms.Button();
      this.grpConnect.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpConnect
      // 
      this.grpConnect.Controls.Add(this.btnManage);
      this.grpConnect.Controls.Add(this.cboInstance);
      this.grpConnect.Controls.Add(this.lblInstance);
      this.grpConnect.Controls.Add(this.cboAddress);
      this.grpConnect.Controls.Add(this.btnConnect);
      this.grpConnect.Controls.Add(this.lblServer);
      this.grpConnect.Location = new System.Drawing.Point(12, 12);
      this.grpConnect.Name = "grpConnect";
      this.grpConnect.Size = new System.Drawing.Size(359, 89);
      this.grpConnect.TabIndex = 0;
      this.grpConnect.TabStop = false;
      this.grpConnect.Text = "grpConnect";
      // 
      // lblServer
      // 
      this.lblServer.AutoSize = true;
      this.lblServer.Location = new System.Drawing.Point(15, 27);
      this.lblServer.Name = "lblServer";
      this.lblServer.Size = new System.Drawing.Size(51, 13);
      this.lblServer.TabIndex = 0;
      this.lblServer.Text = "lblServer:";
      // 
      // btnConnect
      // 
      this.btnConnect.Location = new System.Drawing.Point(278, 22);
      this.btnConnect.Name = "btnConnect";
      this.btnConnect.Size = new System.Drawing.Size(75, 23);
      this.btnConnect.TabIndex = 1;
      this.btnConnect.Text = "btnConnect";
      this.btnConnect.UseVisualStyleBackColor = true;
      this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
      // 
      // cboAddress
      // 
      this.cboAddress.FormattingEnabled = true;
      this.cboAddress.Location = new System.Drawing.Point(87, 24);
      this.cboAddress.Name = "cboAddress";
      this.cboAddress.Size = new System.Drawing.Size(185, 21);
      this.cboAddress.TabIndex = 2;
      // 
      // lblInstance
      // 
      this.lblInstance.AutoSize = true;
      this.lblInstance.Location = new System.Drawing.Point(15, 61);
      this.lblInstance.Name = "lblInstance";
      this.lblInstance.Size = new System.Drawing.Size(61, 13);
      this.lblInstance.TabIndex = 3;
      this.lblInstance.Text = "lblInstance:";
      // 
      // cboInstance
      // 
      this.cboInstance.Enabled = false;
      this.cboInstance.FormattingEnabled = true;
      this.cboInstance.Location = new System.Drawing.Point(87, 58);
      this.cboInstance.Name = "cboInstance";
      this.cboInstance.Size = new System.Drawing.Size(104, 21);
      this.cboInstance.TabIndex = 4;
      // 
      // btnManage
      // 
      this.btnManage.Enabled = false;
      this.btnManage.Location = new System.Drawing.Point(278, 56);
      this.btnManage.Name = "btnManage";
      this.btnManage.Size = new System.Drawing.Size(75, 23);
      this.btnManage.TabIndex = 5;
      this.btnManage.Text = "btnManage";
      this.btnManage.UseVisualStyleBackColor = true;
      this.btnManage.Click += new System.EventHandler(this.btnManage_Click);
      // 
      // frmConnect
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(383, 112);
      this.Controls.Add(this.grpConnect);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmConnect";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "frmConnect";
      this.Load += new System.EventHandler(this.frmConnect_Load);
      this.grpConnect.ResumeLayout(false);
      this.grpConnect.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpConnect;
    private System.Windows.Forms.Label lblServer;
    private System.Windows.Forms.ComboBox cboAddress;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.ComboBox cboInstance;
    private System.Windows.Forms.Label lblInstance;
    private System.Windows.Forms.Button btnManage;
  }
}