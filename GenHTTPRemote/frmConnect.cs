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

  public partial class frmConnect : Form {
    private LocalizationManager _Localization;
    private Setting _Settings;

    private InstanceProvider _Provider;
    private InstanceManager _Manager;

    public frmConnect(LocalizationManager localization, Setting settings) {
      _Localization = localization;
      _Settings = settings;
      InitializeComponent();
    }

    private void frmConnect_Load(object sender, EventArgs e) {
      Setting loc = _Localization.DefaultLocalization["connection"];
      // localize the form
      this.Text = loc["connect"].Value + " ...";
      grpConnect.Text = loc["connection"].Value;
      lblServer.Text = loc["server"].Value + ":";
      btnConnect.Text = loc["connect"].Value;
      lblInstance.Text = loc["instance"].Value;
      btnManage.Text = loc["manage"].Value;
      // load connections
      foreach (Setting child in _Settings["servers"].Children) {
        cboAddress.Items.Add(child.Attributes["ip"] + ":" + child.Attributes["port"]);
      }
      if (cboAddress.Items.Count == 0) cboAddress.Items.Add("127.0.0.1:58000");
      cboAddress.Text = cboAddress.Items[0].ToString();
    }

    private void btnConnect_Click(object sender, EventArgs e) {
      btnConnect.Enabled = false;
      // connect to server
      try {
        _Provider = (InstanceProvider)Activator.GetObject(typeof(InstanceProvider), "tcp://" + cboAddress.Text + "/GenHTTP/InstanceProvider");
        foreach (int instance in _Provider.Instances) cboInstance.Items.Add(instance);
        cboInstance.Enabled = true;
        cboInstance.Text = cboInstance.Items[0].ToString();
        btnManage.Enabled = true;
      }
      catch {
        MessageBox.Show(_Localization.DefaultLocalization["connection"]["errors"]["connection"].Value, _Localization.DefaultLocalization["connection"]["errors"]["connection"].Attributes["title"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        btnConnect.Enabled = true;
      }
    }

    private void btnManage_Click(object sender, EventArgs e) {
      btnManage.Enabled = false;
      // retrieve pw
      frmPassword frm = new frmPassword(_Localization);
      frm.ShowDialog();
      string password = Hash.HashString(frm.Password);
      // connect to instance
      try {
        _Manager = _Provider.ManageInstance(Convert.ToInt32(cboInstance.Text), password);
        this.Close();
      }
      catch (Exception ex) {
        MessageBox.Show(_Localization.DefaultLocalization["connection"]["errors"]["instance"].Value + Environment.NewLine + Environment.NewLine + ex.Message, _Localization.DefaultLocalization["connection"]["errors"]["instance"].Attributes["title"], MessageBoxButtons.OK, MessageBoxIcon.Error);
        btnManage.Enabled = true;
      }
    }

    #region get-/setters

    internal InstanceManager Manager {
      get { return _Manager; }
    }

    #endregion

  }

}
