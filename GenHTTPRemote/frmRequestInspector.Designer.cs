namespace GenHTTP.Remoting {
  partial class frmRequestInspector {
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
      System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("grpGeneral", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("grpConnection", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("grpProject", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("grpGetFields", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("grpPostFields", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("grpCookies", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("grpRedirects", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup8 = new System.Windows.Forms.ListViewGroup("grpGeneral", System.Windows.Forms.HorizontalAlignment.Left);
      System.Windows.Forms.ListViewGroup listViewGroup9 = new System.Windows.Forms.ListViewGroup("grpConnection", System.Windows.Forms.HorizontalAlignment.Left);
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRequestInspector));
      this.tabBundle = new System.Windows.Forms.TabControl();
      this.tabRequest = new System.Windows.Forms.TabPage();
      this.tabResponse = new System.Windows.Forms.TabPage();
      this.listRequest = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colValue = new System.Windows.Forms.ColumnHeader();
      this.listResponse = new System.Windows.Forms.ListView();
      this.colRespName = new System.Windows.Forms.ColumnHeader();
      this.colRespValue = new System.Windows.Forms.ColumnHeader();
      this.tabBundle.SuspendLayout();
      this.tabRequest.SuspendLayout();
      this.tabResponse.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabBundle
      // 
      this.tabBundle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabBundle.Controls.Add(this.tabRequest);
      this.tabBundle.Controls.Add(this.tabResponse);
      this.tabBundle.Location = new System.Drawing.Point(12, 12);
      this.tabBundle.Name = "tabBundle";
      this.tabBundle.SelectedIndex = 0;
      this.tabBundle.Size = new System.Drawing.Size(637, 422);
      this.tabBundle.TabIndex = 0;
      // 
      // tabRequest
      // 
      this.tabRequest.Controls.Add(this.listRequest);
      this.tabRequest.Location = new System.Drawing.Point(4, 22);
      this.tabRequest.Name = "tabRequest";
      this.tabRequest.Padding = new System.Windows.Forms.Padding(3);
      this.tabRequest.Size = new System.Drawing.Size(629, 396);
      this.tabRequest.TabIndex = 0;
      this.tabRequest.Text = "tabRequest";
      this.tabRequest.UseVisualStyleBackColor = true;
      // 
      // tabResponse
      // 
      this.tabResponse.Controls.Add(this.listResponse);
      this.tabResponse.Location = new System.Drawing.Point(4, 22);
      this.tabResponse.Name = "tabResponse";
      this.tabResponse.Padding = new System.Windows.Forms.Padding(3);
      this.tabResponse.Size = new System.Drawing.Size(629, 396);
      this.tabResponse.TabIndex = 1;
      this.tabResponse.Text = "tabResponse";
      this.tabResponse.UseVisualStyleBackColor = true;
      // 
      // listRequest
      // 
      this.listRequest.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colValue});
      listViewGroup1.Header = "grpGeneral";
      listViewGroup1.Name = "grpGeneral";
      listViewGroup2.Header = "grpConnection";
      listViewGroup2.Name = "grpConnection";
      listViewGroup3.Header = "grpProject";
      listViewGroup3.Name = "grpProject";
      listViewGroup4.Header = "grpGetFields";
      listViewGroup4.Name = "grpGetFields";
      listViewGroup5.Header = "grpPostFields";
      listViewGroup5.Name = "grpPostFields";
      listViewGroup6.Header = "grpCookies";
      listViewGroup6.Name = "grpCookies";
      listViewGroup7.Header = "grpRedirects";
      listViewGroup7.Name = "grpRedirects";
      this.listRequest.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4,
            listViewGroup5,
            listViewGroup6,
            listViewGroup7});
      this.listRequest.Location = new System.Drawing.Point(6, 6);
      this.listRequest.Name = "listRequest";
      this.listRequest.Size = new System.Drawing.Size(617, 384);
      this.listRequest.TabIndex = 0;
      this.listRequest.UseCompatibleStateImageBehavior = false;
      this.listRequest.View = System.Windows.Forms.View.Details;
      // 
      // colName
      // 
      this.colName.Text = "colName";
      this.colName.Width = 132;
      // 
      // colValue
      // 
      this.colValue.Text = "colValue";
      this.colValue.Width = 448;
      // 
      // listResponse
      // 
      this.listResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listResponse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colRespName,
            this.colRespValue});
      listViewGroup8.Header = "grpGeneral";
      listViewGroup8.Name = "grpGeneral";
      listViewGroup9.Header = "grpConnection";
      listViewGroup9.Name = "grpConnection";
      this.listResponse.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup8,
            listViewGroup9});
      this.listResponse.Location = new System.Drawing.Point(6, 6);
      this.listResponse.Name = "listResponse";
      this.listResponse.Size = new System.Drawing.Size(617, 384);
      this.listResponse.TabIndex = 0;
      this.listResponse.UseCompatibleStateImageBehavior = false;
      this.listResponse.View = System.Windows.Forms.View.Details;
      // 
      // colRespName
      // 
      this.colRespName.Text = "colRespName";
      this.colRespName.Width = 132;
      // 
      // colRespValue
      // 
      this.colRespValue.Text = "colRespValue";
      this.colRespValue.Width = 448;
      // 
      // frmRequestInspector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(661, 446);
      this.Controls.Add(this.tabBundle);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "frmRequestInspector";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "frmRequestInspector";
      this.Load += new System.EventHandler(this.frmRequestInspector_Load);
      this.tabBundle.ResumeLayout(false);
      this.tabRequest.ResumeLayout(false);
      this.tabResponse.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabBundle;
    private System.Windows.Forms.TabPage tabRequest;
    private System.Windows.Forms.TabPage tabResponse;
    private System.Windows.Forms.ListView listRequest;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.ColumnHeader colValue;
    private System.Windows.Forms.ListView listResponse;
    private System.Windows.Forms.ColumnHeader colRespName;
    private System.Windows.Forms.ColumnHeader colRespValue;

  }
}