using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GenHTTP.Localization;

namespace GenHTTP.Remoting {

  public partial class frmPassword : Form {
    private LocalizationManager _Localization;

    public frmPassword(LocalizationManager localization) {
      _Localization = localization;
      InitializeComponent();
    }

    private void frmPassword_Load(object sender, EventArgs e) {
      this.Text = _Localization.DefaultLocalization["connection"]["password"].Value;
    }

    private void txtPassword_KeyDown(object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.Return) {
        this.Close();
      }
    }

    public string Password {
      get { return txtPassword.Text; }
    }

  }

}
