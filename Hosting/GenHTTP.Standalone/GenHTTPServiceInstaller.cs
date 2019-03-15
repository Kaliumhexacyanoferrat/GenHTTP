using System;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;

namespace GenHTTP {

  /// <summary>
  /// Installs the GenHTTP service.
  /// </summary>
  [RunInstaller(true)]
  public class GenHTTPServiceInstaller : Installer {
    private ServiceProcessInstaller ServiceProcessInstaller1;
    private ServiceInstaller ServiceInstaller1;

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public GenHTTPServiceInstaller() {
      InitializeComponent();
    }

    private void InitializeComponent() {
      this.ServiceProcessInstaller1 = new ServiceProcessInstaller();
      this.ServiceProcessInstaller1.Account = ServiceAccount.LocalSystem;
      this.ServiceProcessInstaller1.Username = null;
      this.ServiceProcessInstaller1.Password = null;
      this.ServiceInstaller1 = new ServiceInstaller();
      this.ServiceInstaller1.Description = "Small HTTP webserver providing a platform for web applications";
      this.ServiceInstaller1.DisplayName = "GenHTTP Webserver";
      this.ServiceInstaller1.ServiceName = "GenHTTP";
      this.ServiceInstaller1.StartType = ServiceStartMode.Manual;
      this.Installers.AddRange(new Installer[] { this.ServiceProcessInstaller1, this.ServiceInstaller1 });
      this.ServiceInstaller1.AfterInstall += new InstallEventHandler(ServiceInstaller1_AfterInstall);
    }

    void ServiceInstaller1_AfterInstall(object sender, InstallEventArgs e) {
      RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\GenHTTP", true);
      key.SetValue("ImagePath", key.GetValue("ImagePath").ToString() + " -s");
      key.Close();
    }

  }

}