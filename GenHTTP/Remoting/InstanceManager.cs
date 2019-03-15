using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;

namespace GenHTTP.Remoting {

  /// <summary>
  /// Allows you to manage a server instance remotely.
  /// </summary>
  [Serializable]
  public class InstanceManager : MarshalByRefObject {
    private Server _Server;
    private Log _Log;
    private List<HandledRequest> _Bundles;

    /// <summary>
    /// Create a new instance manager for the given server.
    /// </summary>
    /// <param name="server">The server to manage</param>
    public InstanceManager(Server server) {
      _Server = server;
      // find a free log file
      if (!File.Exists(_Server.Path + "logs/remote_" + Log.DateTimeString + ".log")) {
        _Log = new Log(_Server.Path + "logs/remote_" + Log.DateTimeString + ".log", false);
      }
      else {
        int i = 2;
        while (File.Exists(_Server.Path + "logs/remote_" + Log.DateTimeString + "_" + i + ".log")) i++;
        _Log = new Log(_Server.Path + "logs/remote_" + Log.DateTimeString + "_" + i + ".log", false);
      }
      // log info
      _Log.WriteLine("");
      _Log.WriteLine("GenHTTP Remote Interface Session");
      _Log.WriteLine("");
      _Bundles = new List<HandledRequest>();
      _Server.OnRequestHandled += new RequestHandled(_Server_OnRequestHandled);
    }

    void _Server_OnRequestHandled(HttpRequest request, HttpResponse response) {
      _Bundles.Add(new HandledRequest(request, response));
    }

    /// <summary>
    /// The number of pending requests.
    /// </summary>
    public int RequestCount {
      get { return _Bundles.Count; }
    }

    /// <summary>
    /// Retrieve the requests which were sent last.
    /// </summary>
    public HandledRequest[] Requests {
      get {
        HandledRequest[] ret = new HandledRequest[_Bundles.Count];
        _Bundles.CopyTo(0, ret, 0, ret.Length);
        _Bundles.Clear();
        return ret;
      }
    }


    /// <summary>
    /// Retrieve active projects.
    /// </summary>
    public ProjectCollection ActiveProjects {
      get {
        _Log.WriteLine("Retrieving active projects ...");
        return _Server.Projects;
      }
    }

    /// <summary>
    /// Unload a project.
    /// </summary>
    /// <param name="project">The project to unload</param>
    public void UnloadProject(string project) {
      _Log.WriteLine("Unloading project '" + project + "' ...");
      _Server.Projects.Unload(project);
    }

    /// <summary>
    /// Load a project from an assembly.
    /// </summary>
    /// <param name="file">The assembly to load from</param>
    /// <param name="completeClassName">The type to instantiate</param>
    public void LoadProject(string file, string completeClassName) {
      _Log.WriteLine("Loading project '" + completeClassName + "' from file '" + file + "' ...");
      _Server.Projects.Load(file, completeClassName);
    }

    /// <summary>
    /// The path the server is running in.
    /// </summary>
    public string Path {
      get { return _Server.Path; }
    }

  }
  

}
