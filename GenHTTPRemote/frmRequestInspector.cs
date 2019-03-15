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
 
  public partial class frmRequestInspector : Form {
    private HandledRequest _Bundle;
    private LocalizationManager _Localization;

    public frmRequestInspector(HandledRequest bundle, LocalizationManager localization) {
      _Bundle = bundle;
      _Localization = localization;
      InitializeComponent();
    }

    private void frmRequestInspector_Load(object sender, EventArgs e) {
      // localize
      Setting loc = _Localization.DefaultLocalization["inspector"];
      this.Text = loc["title"].Value;
      tabRequest.Text = loc["request"].Value;
      tabResponse.Text = loc["response"].Value;
      listRequest.Groups[0].Header = loc["general"].Value;
      listRequest.Groups[1].Header = loc["connection"].Value;
      listRequest.Groups[2].Header = loc["project"].Value;
      listRequest.Groups[3].Header = loc["getfields"].Value;
      listRequest.Groups[4].Header = loc["postfields"].Value;
      listRequest.Groups[5].Header = loc["cookies"].Value;
      listRequest.Groups[6].Header = loc["redirects"].Value;
      listResponse.Groups[0].Header = loc["general"].Value;
      listResponse.Groups[1].Header = loc["connection"].Value;
      colName.Text = colRespName.Text = loc["name"].Value;
      colValue.Text = colRespValue.Text = loc["value"].Value;
      // show request
      AddToGroup(loc["version"].Value, 0).SubItems.Add(_Bundle.Request.ProtocolType.ToString().Replace("_1_1", " 1.1").Replace("_1_0", " 1.0"));
      AddToGroup(loc["file"].Value, 0).SubItems.Add(_Bundle.Request.File);
      AddToGroup(loc["method"].Value, 0).SubItems.Add(_Bundle.Request.Type.ToString());
      AddToGroup(loc["compression"].Value, 0).SubItems.Add(_Bundle.Request.CompressionAvailable.ToString());
      AddToGroup(loc["referer"].Value, 0).SubItems.Add(_Bundle.Request.Referer);
      AddToGroup(loc["agent"].Value, 0).SubItems.Add(_Bundle.Request.UserAgent);

      AddToGroup(loc["ip"].Value, 1).SubItems.Add(_Bundle.Request.Handler.IP);
      AddToGroup(loc["port"].Value, 1).SubItems.Add(_Bundle.Request.Handler.Port.ToString());

      if (_Bundle.Request.Project != null) {
        AddToGroup(loc["name"].Value, 2).SubItems.Add(_Bundle.Request.Project.Name);
        AddToGroup(loc["projectversion"].Value, 2).SubItems.Add(_Bundle.Request.Project.Version);
      }

      if (_Bundle.Request.GetFields.Count > 0) {
        foreach (KeyValuePair<string, string> pair in _Bundle.Request.GetFields) {
          AddToGroup(pair.Key, 3).SubItems.Add(pair.Value);
        }
      }

      if (_Bundle.Request.PostFields.Count > 0) {
        foreach (KeyValuePair<string, string> pair in _Bundle.Request.PostFields) {
          AddToGroup(pair.Key, 4).SubItems.Add(pair.Value);
        }
      }

      if (_Bundle.Request.Cookies.Count > 0) {
        foreach (HttpCookie cookie in _Bundle.Request.Cookies) {
          AddToGroup(cookie.Name, 5).SubItems.Add(cookie.Value);
        }
      }

      if (_Bundle.Request.Redirected) {
        foreach (string address in _Bundle.Request.RedirectedFrom) {
          AddToGroup(loc["from"].Value, 6).SubItems.Add(address);
        }
      }
      // show response
      AddToResponseGroup(loc["responsecode"].Value, 0).SubItems.Add(_Bundle.Response.Header.Type.ToString());
      AddToResponseGroup(loc["contenttype"].Value, 0).SubItems.Add(_Bundle.Response.Header.ContentType.ToString());

      AddToResponseGroup(loc["sent"].Value, 1).SubItems.Add(_Bundle.Response.ContentLenght.ToString());
      AddToResponseGroup(loc["loadtime"].Value, 1).SubItems.Add(_Bundle.Response.LoadTime.ToString());
      AddToResponseGroup(loc["compressed"].Value, 1).SubItems.Add(_Bundle.Response.UseCompression.ToString());
      AddToResponseGroup(loc["close"].Value, 1).SubItems.Add(_Bundle.Response.Header.CloseConnection.ToString());
    }

    private ListViewItem AddToGroup(string text, int group) {
      ListViewItem item = listRequest.Items.Add(text);
      item.Group = listRequest.Groups[group];
      return item;
    }

    private ListViewItem AddToResponseGroup(string text, int group) {
      ListViewItem item = listResponse.Items.Add(text);
      item.Group = listResponse.Groups[group];
      return item;
    }

  }

}
