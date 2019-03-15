using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GenHTTP.Localization;
using GenHTTP.Utilities;

namespace GenHTTP.Remoting {

  public partial class frmLoadProject : Form {
    private LocalizationManager _Localization;
    private InstanceManager _Manager;

    public frmLoadProject(LocalizationManager localization, InstanceManager manager) {
      _Localization = localization;
      _Manager = manager;
      InitializeComponent();
    }

    private void frmLoadProject_Load(object sender, EventArgs e) {
      // localize this form
      Setting loc = _Localization.DefaultLocalization["main"]["projects"]["loadform"];
      this.Text = loc["title"].Value;
      lblFile.Text = loc["file"].Value;
      lblType.Text = loc["type"].Value;
      btnLoad.Text = loc["load"].Value;
      // add some helpers
      txtFile.Text = _Manager.Path;
    }

    private void btnLoad_Click(object sender, EventArgs e) {
      try {
        _Manager.LoadProject(txtFile.Text, txtType.Text);
        this.Close();
      }
      catch (Exception ex) {
        Setting loc = _Localization.DefaultLocalization["main"]["projects"]["loadform"]["errors"]["load"];
        MessageBox.Show(loc.Value + Environment.NewLine + Environment.NewLine + ex.Message, loc.Attributes["title"], MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }

}
