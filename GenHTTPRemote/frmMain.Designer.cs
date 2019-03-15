namespace GenHTTP.Remoting {
  partial class frmMain {
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabRequests = new System.Windows.Forms.TabPage();
      this.listRequests = new System.Windows.Forms.ListView();
      this.colIP = new System.Windows.Forms.ColumnHeader();
      this.colType = new System.Windows.Forms.ColumnHeader();
      this.colFile = new System.Windows.Forms.ColumnHeader();
      this.colSize = new System.Windows.Forms.ColumnHeader();
      this.colTime = new System.Windows.Forms.ColumnHeader();
      this.imgList = new System.Windows.Forms.ImageList(this.components);
      this.tabProjects = new System.Windows.Forms.TabPage();
      this.btnLoad = new System.Windows.Forms.Button();
      this.btnUnload = new System.Windows.Forms.Button();
      this.listProjects = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colVersion = new System.Windows.Forms.ColumnHeader();
      this.colStartup = new System.Windows.Forms.ColumnHeader();
      this.tmrRequests = new System.Windows.Forms.Timer(this.components);
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.strFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.strCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.strLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.tabControl1.SuspendLayout();
      this.tabRequests.SuspendLayout();
      this.tabProjects.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabRequests);
      this.tabControl1.Controls.Add(this.tabProjects);
      this.tabControl1.Location = new System.Drawing.Point(12, 27);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(798, 383);
      this.tabControl1.TabIndex = 0;
      // 
      // tabRequests
      // 
      this.tabRequests.Controls.Add(this.listRequests);
      this.tabRequests.Location = new System.Drawing.Point(4, 22);
      this.tabRequests.Name = "tabRequests";
      this.tabRequests.Padding = new System.Windows.Forms.Padding(3);
      this.tabRequests.Size = new System.Drawing.Size(790, 357);
      this.tabRequests.TabIndex = 0;
      this.tabRequests.Text = "tabRequests";
      this.tabRequests.UseVisualStyleBackColor = true;
      // 
      // listRequests
      // 
      this.listRequests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listRequests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIP,
            this.colType,
            this.colFile,
            this.colSize,
            this.colTime});
      this.listRequests.FullRowSelect = true;
      this.listRequests.GridLines = true;
      this.listRequests.Location = new System.Drawing.Point(6, 6);
      this.listRequests.Name = "listRequests";
      this.listRequests.Size = new System.Drawing.Size(778, 345);
      this.listRequests.SmallImageList = this.imgList;
      this.listRequests.TabIndex = 0;
      this.listRequests.UseCompatibleStateImageBehavior = false;
      this.listRequests.View = System.Windows.Forms.View.Details;
      this.listRequests.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listRequests_MouseDoubleClick);
      // 
      // colIP
      // 
      this.colIP.Text = "colIP";
      this.colIP.Width = 119;
      // 
      // colType
      // 
      this.colType.Text = "colType";
      this.colType.Width = 46;
      // 
      // colFile
      // 
      this.colFile.Text = "colFile";
      this.colFile.Width = 418;
      // 
      // colSize
      // 
      this.colSize.Text = "colSize";
      this.colSize.Width = 69;
      // 
      // colTime
      // 
      this.colTime.Text = "colTime";
      this.colTime.Width = 90;
      // 
      // imgList
      // 
      this.imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList.ImageStream")));
      this.imgList.TransparentColor = System.Drawing.Color.Transparent;
      this.imgList.Images.SetKeyName(0, "OK");
      this.imgList.Images.SetKeyName(1, "Failed");
      // 
      // tabProjects
      // 
      this.tabProjects.Controls.Add(this.btnLoad);
      this.tabProjects.Controls.Add(this.btnUnload);
      this.tabProjects.Controls.Add(this.listProjects);
      this.tabProjects.Location = new System.Drawing.Point(4, 22);
      this.tabProjects.Name = "tabProjects";
      this.tabProjects.Size = new System.Drawing.Size(790, 335);
      this.tabProjects.TabIndex = 1;
      this.tabProjects.Text = "tabProjects";
      this.tabProjects.UseVisualStyleBackColor = true;
      // 
      // btnLoad
      // 
      this.btnLoad.Location = new System.Drawing.Point(695, 32);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(92, 23);
      this.btnLoad.TabIndex = 2;
      this.btnLoad.Text = "btnLoad";
      this.btnLoad.UseVisualStyleBackColor = true;
      this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
      // 
      // btnUnload
      // 
      this.btnUnload.Location = new System.Drawing.Point(695, 3);
      this.btnUnload.Name = "btnUnload";
      this.btnUnload.Size = new System.Drawing.Size(92, 23);
      this.btnUnload.TabIndex = 1;
      this.btnUnload.Text = "btnUnload";
      this.btnUnload.UseVisualStyleBackColor = true;
      this.btnUnload.Click += new System.EventHandler(this.btnUnload_Click);
      // 
      // listProjects
      // 
      this.listProjects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colVersion,
            this.colStartup});
      this.listProjects.FullRowSelect = true;
      this.listProjects.GridLines = true;
      this.listProjects.Location = new System.Drawing.Point(3, 3);
      this.listProjects.Name = "listProjects";
      this.listProjects.Size = new System.Drawing.Size(686, 329);
      this.listProjects.TabIndex = 0;
      this.listProjects.UseCompatibleStateImageBehavior = false;
      this.listProjects.View = System.Windows.Forms.View.Details;
      // 
      // colName
      // 
      this.colName.Text = "colName";
      this.colName.Width = 143;
      // 
      // colVersion
      // 
      this.colVersion.Text = "colVersion";
      this.colVersion.Width = 90;
      // 
      // colStartup
      // 
      this.colStartup.Text = "colStartup";
      this.colStartup.Width = 426;
      // 
      // tmrRequests
      // 
      this.tmrRequests.Interval = 5000;
      this.tmrRequests.Tick += new System.EventHandler(this.tmrRequests_Tick);
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.strFileToolStripMenuItem,
            this.strLanguageToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(822, 24);
      this.menuStrip1.TabIndex = 1;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // strFileToolStripMenuItem
      // 
      this.strFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.strCloseToolStripMenuItem});
      this.strFileToolStripMenuItem.Name = "strFileToolStripMenuItem";
      this.strFileToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
      this.strFileToolStripMenuItem.Text = "strFile";
      // 
      // strCloseToolStripMenuItem
      // 
      this.strCloseToolStripMenuItem.Name = "strCloseToolStripMenuItem";
      this.strCloseToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.strCloseToolStripMenuItem.Text = "strClose";
      this.strCloseToolStripMenuItem.Click += new System.EventHandler(this.strCloseToolStripMenuItem_Click);
      // 
      // strLanguageToolStripMenuItem
      // 
      this.strLanguageToolStripMenuItem.Name = "strLanguageToolStripMenuItem";
      this.strLanguageToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
      this.strLanguageToolStripMenuItem.Text = "strLanguage";
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(822, 422);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.menuStrip1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "frmMain";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "frmMain";
      this.Load += new System.EventHandler(this.frmMain_Load);
      this.tabControl1.ResumeLayout(false);
      this.tabRequests.ResumeLayout(false);
      this.tabProjects.ResumeLayout(false);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabRequests;
    private System.Windows.Forms.ListView listRequests;
    private System.Windows.Forms.ColumnHeader colIP;
    private System.Windows.Forms.ColumnHeader colFile;
    private System.Windows.Forms.ColumnHeader colType;
    private System.Windows.Forms.ColumnHeader colSize;
    private System.Windows.Forms.ColumnHeader colTime;
    private System.Windows.Forms.ImageList imgList;
    private System.Windows.Forms.TabPage tabProjects;
    private System.Windows.Forms.ListView listProjects;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.ColumnHeader colVersion;
    private System.Windows.Forms.ColumnHeader colStartup;
    private System.Windows.Forms.Button btnUnload;
    private System.Windows.Forms.Button btnLoad;
    private System.Windows.Forms.Timer tmrRequests;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem strFileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem strCloseToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem strLanguageToolStripMenuItem;


  }
}

