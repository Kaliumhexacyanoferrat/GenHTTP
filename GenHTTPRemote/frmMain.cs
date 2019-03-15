using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using GenHTTP.Localization;
using GenHTTP.Utilities;

namespace GenHTTP.Remoting {

  public partial class frmMain : Form {
    private LocalizationManager _Localization;
    private Setting _Settings;
    private string _Path;

    private InstanceManager _Manager;

    public frmMain() {
      InitializeComponent();
    }

    private void frmMain_Load(object sender, EventArgs e) {
      // set some security settings
      // more info: http://groups.google.ca/group/microsoft.public.dotnet.languages.csharp/browse_frm/thread/37bf4735e4f3cf20/c6feb58462b45bb7?lnk=st&q=Remoting+Serialization+Security+Stoitcho&rnum=1&hl=en#c6feb58462b45bb7
      BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
      BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
      serverProvider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
      Hashtable props = new Hashtable();
      props["port"] = 0;
      props["typeFilterLevel"] = TypeFilterLevel.Full;
      TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
      ChannelServices.RegisterChannel(channel, false); 
      // read settings and initialize this form
      _Settings = Setting.FromXml(Path + "config/remote_settings.xml");
      _Localization = new LocalizationManager(_Settings["localization"]);
      // add languages
      foreach (Localization.Localization language in _Localization.AvailableLocalizations) {
        if (language.Name == "common") continue;
        ToolStripItem strip = strLanguageToolStripMenuItem.DropDownItems.Add(language.Name.Substring(0, 1).ToUpper() + language.Name.Substring(1));
        strip.Click += new EventHandler(strip_Click);
      }
      Localize();
      // show connection form
      this.Show();
      frmConnect frm = new frmConnect(_Localization, _Settings);
      frm.ShowDialog(this);
      if (frm.Manager == null) Environment.Exit(1);
      // retrieve the data from the manager
      _Manager = frm.Manager;
      // load projects
      LoadProjects();
      // enable timer
      tmrRequests.Enabled = true;
    }

    void strip_Click(object sender, EventArgs e) {
      ToolStripItem strip = (ToolStripItem)sender;
      _Localization.DefaultLocalization = _Localization[strip.Text];
      Localize();
      _Settings.ToXml();
    }

    private void LoadProjects() {
      listProjects.Items.Clear();
      foreach (IProject project in _Manager.ActiveProjects) {
        ListViewItem item = listProjects.Items.Add(project.Name);
        item.SubItems.Add(project.Version);
        item.SubItems.Add(project.LocalFolder);
        item.Tag = project;
      }
    }

    /// <summary>
    /// Localize this form.
    /// </summary>
    internal void Localize() {
      this.Text = _Localization.Common["title"].Value;
      Setting loc = _Localization.DefaultLocalization["main"];
      tabRequests.Text = loc["requests"]["requests"].Value;
      colIP.Text = loc["requests"]["ip"].Value;
      colFile.Text = loc["requests"]["file"].Value;
      colType.Text = loc["requests"]["type"].Value;
      colSize.Text = loc["requests"]["size"].Value; 
      colTime.Text = loc["requests"]["loadtime"].Value;

      tabProjects.Text = loc["projects"]["projects"].Value;
      colName.Text = loc["projects"]["name"].Value;
      colVersion.Text = loc["projects"]["version"].Value;
      colStartup.Text = loc["projects"]["startup"].Value;
      btnUnload.Text = loc["projects"]["unload"].Value;
      btnLoad.Text = loc["projects"]["load"].Value;

      strFileToolStripMenuItem.Text = loc["menu"]["file"].Value;
      strCloseToolStripMenuItem.Text = loc["menu"]["close"].Value;
      strLanguageToolStripMenuItem.Text = loc["menu"]["lang"].Value;
    }

    #region get-/setters

    /// <summary>
    /// Retrieve the current path.
    /// </summary>
    public string Path {
      get {
        if (_Path != null) return _Path;
        FileInfo file = new FileInfo(Assembly.GetExecutingAssembly().Location);
        return _Path = file.Directory.FullName.Replace("\\", "/") + "/";
      }
    }

    #endregion

    private void listRequests_MouseDoubleClick(object sender, MouseEventArgs e) {
      if (listRequests.SelectedItems.Count > 0) {
        ListViewItem item = listRequests.SelectedItems[0];
        frmRequestInspector frm = new frmRequestInspector((HandledRequest)item.Tag, _Localization);
        frm.ShowDialog(this);
      }
    }

    private void btnUnload_Click(object sender, EventArgs e) {
      if (listProjects.SelectedItems.Count > 0) {
        IProject project = (IProject)listProjects.SelectedItems[0].Tag;
        try {
          _Manager.UnloadProject(project.Name);
          listProjects.Items.Remove(listProjects.SelectedItems[0]);
        }
        catch (Exception ex) {
          MessageBox.Show(_Localization.DefaultLocalization["main"]["projects"]["errors"]["unload"].Value + Environment.NewLine + Environment.NewLine + ex.Message, _Localization.DefaultLocalization["main"]["projects"]["errors"]["unload"].Attributes["title"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private void btnLoad_Click(object sender, EventArgs e) {
      frmLoadProject frm = new frmLoadProject(_Localization, _Manager);
      frm.Show(this);
      LoadProjects();
    }

    private void tmrRequests_Tick(object sender, EventArgs e) {
      if (_Manager.RequestCount > 0) {
        foreach (HandledRequest bundle in _Manager.Requests) {
          ListViewItem item = listRequests.Items.Add(bundle.Request.Handler.IP);
          item.Tag = bundle;
          item.SubItems.Add(HttpRequest.GetRequestTypeName(bundle.Request.Type));
          item.SubItems.Add(bundle.Request.File);
          item.SubItems.Add(bundle.Response.ContentLenght.ToString());
          item.SubItems.Add(bundle.Response.LoadTime.ToString());
          if (bundle.Response.Header.Type == ResponseType.OK)
            item.ImageIndex = 0;
          else
            item.ImageIndex = 1;
          item.EnsureVisible();
        }
      }
    }

    private void strCloseToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Exit();
    }


  }

}
