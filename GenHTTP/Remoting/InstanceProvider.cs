using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP;

namespace GenHTTP.Remoting {

  /// <summary>
  /// Allows you to retrieve all server instances running in this process.
  /// </summary>
  public class InstanceProvider : MarshalByRefObject {

    /// <summary>
    /// Retrieve the instances of the process.
    /// </summary>
    public int[] Instances {
      get {
        return Server.Instances.Keys.ToArray<int>();
      }
    }

    /// <summary>
    /// Try to manage a server instance.
    /// </summary>
    /// <param name="instance">The port number of the instance</param>
    /// <param name="password">The remoting password</param>
    /// <returns></returns>
    public InstanceManager ManageInstance(int instance, string password) {
      if (Server.Instances.ContainsKey(instance)) {
        if (Server.Instances[instance].Configuration.RemotingPassword != password) throw new Exception("Invalid password");
        return new InstanceManager(Server.Instances[instance]);
      }
      throw new Exception("This instance could not be found on this server");
    }

  }

}
