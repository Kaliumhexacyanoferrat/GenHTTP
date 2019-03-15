/*

Updated: 2009/10/29

2009/10/29  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP {
  
  /// <summary>
  /// A configuration object used by the GenHTTP webserver.
  /// </summary>
  /// <remarks>
  /// 7 plus or minus ... 23.
  /// </remarks>
  [Serializable]
  public class Configuration : MarshalByRefObject {
    private ushort _SocketPort = 80;
    private ushort _SocketBacklog = 100;
    private ushort _SocketTimeout = 15;
    private int _SocketBuffersReceive = 1520;
    private int _SocketThreadsMax = 256;
    private bool _SocketCongestionPrevention = true;
    private float _SocketCongestionLimit = 0.9f;
    private ProtocolType _HttpProtocolVersion = ProtocolType.Http_1_1;
    private bool _HttpCompressionEnabled = true;
    private string _HttpCompressionAlgorithm = "gzip";
    private uint _HttpCompressionLimit = 1048576;
    private List<RewriteConfiguration> _HttpRewrites;
    private bool _HttpVHostsEnabled = false;
    private Dictionary<string, string> _HttpVHosts;
    private string _DocumentsRoot = "./projects/";
    private string _DocumentsHelper = "./GenHTTP.Utilities.StandardHelper.dll";
    private string _DocumentsHelperType = "";
    private bool _LoggingConsole = true;
    private bool _RemotingEnabled = true;
    private ushort _RemotingPort = 58000;
    private string _RemotingPassword = "600A55763B4D4420";
    private ushort _MiscRegexSize = 15;
    private ushort _MiscTimerIntervall = 300;

    private Collection<KeyValuePair<string, string>> _VHosts;
    private Collection<RewriteConfiguration> _Rewrites;

    #region Constructors

    /// <summary>
    /// Default configuration.
    /// </summary>
    public Configuration() {
      _HttpRewrites = new List<RewriteConfiguration>();
      _HttpVHosts = new Dictionary<string, string>();
      GenerateCollectionFromLists();
    }

    /// <summary>
    /// Pre-generate collections which should be much faster.
    /// </summary>
    private void GenerateCollectionFromLists() {
      _VHosts = new Collection<KeyValuePair<string, string>>(new List<KeyValuePair<string, string>>(_HttpVHosts));
      _Rewrites = new Collection<RewriteConfiguration>(_HttpRewrites);
    }

    /// <summary>
    /// Read a configuration from a XML file.
    /// </summary>
    /// <param name="file">The file to read</param>
    public Configuration(string file) : this(Setting.FromXml(file)) {

    }

    /// <summary>
    /// Read a configuration from an opened XML reader.
    /// </summary>
    /// <param name="node">The node to read</param>
    public Configuration(Setting node) : this() {
      _SocketPort = node["socket"]["port"].ConvertTo<ushort>(_SocketPort);
      _SocketBacklog = node["socket"]["backlog"].ConvertTo<ushort>(_SocketBacklog);
      _SocketTimeout = node["socket"]["timeout"].ConvertTo<ushort>(_SocketTimeout);
      _SocketBuffersReceive = node["socket"]["buffers"]["receive"].ConvertTo<int>(_SocketBuffersReceive);
      _SocketThreadsMax = node["socket"]["threads"]["max"].ConvertTo<int>(_SocketThreadsMax);
      _SocketCongestionPrevention = (node["socket"]["congestion"].Attributes["prevention"] == "true");
      _SocketCongestionLimit = node["socket"]["congestion"]["limit"].ConvertTo<float>(_SocketCongestionLimit);
      _HttpProtocolVersion = (node["http"]["protocol"].Attributes["version"] == "1.1") ? ProtocolType.Http_1_1 : ProtocolType.Http_1_0;
      _HttpCompressionEnabled = (node["http"]["compression"].Attributes["enabled"] == "true");
      _HttpCompressionAlgorithm = node["http"]["compression"]["algorithm"].Value;
      _HttpCompressionLimit = node["http"]["compression"]["limit"].ConvertTo<uint>(_HttpCompressionLimit);
      foreach (Setting rewrite in node["http"]["rewrites"].Children) {
        _HttpRewrites.Add(new RewriteConfiguration(rewrite.Attributes["url"], rewrite.Attributes["to"], rewrite.Attributes["regex"] == "true"));
      }
      _HttpVHostsEnabled = (node["http"]["vhosts"].Attributes["enabled"] == "true");
      foreach (Setting vhost in node["http"]["vhosts"].Children) {
        AddVHost(vhost.Attributes["domain"], vhost.Attributes["project"]);
      }
      _DocumentsRoot = node["documents"]["root"].Value;
      _DocumentsHelper = node["documents"]["helper"].Value;
      _DocumentsHelperType = node["documents"]["helper"].Attributes["type"];
      _LoggingConsole = (node["logging"].Attributes["console"] == "true");
      _RemotingEnabled = (node["remoting"].Attributes["enabled"] == "true");
      _RemotingPort = node["remoting"]["port"].ConvertTo<ushort>(_RemotingPort);
      _RemotingPassword = node["remoting"]["password"].Value;
      try {
        _MiscRegexSize = Convert.ToUInt16(node["misc"]["regex"].Attributes["size"]);
      }
      catch {}
      try {
        _MiscTimerIntervall = Convert.ToUInt16(node["misc"]["timer"].Attributes["intervall"]);
      }
      catch { }
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Specifies the port number on which the server is listening on.
    /// </summary>
    public ushort Port {
      get { return _SocketPort; }
      set { _SocketPort = value; }
    }

    /// <summary>
    /// Number of slots in the connection buffer. A high value will
    /// lead to increased memory consumption. Increase this value, if there
    /// are many clients connecting to your server.
    /// </summary>
    public ushort Backlog {
      get { return _SocketBacklog; }
      set { _SocketBacklog = value; }
    }

    /// <summary>
    /// The time in seconds, the server will leave a connection to the client
    /// open, waiting for new requests from the client's browser. This will
    /// reserve a slot for the client in the thread pool. If this value is too
    /// high, you may meet congestion problems.
    /// </summary>
    public ushort Timeout {
      get { return _SocketTimeout; }
      set { _SocketTimeout = value; }
    }

    /// <summary>
    /// The size of the buffer used to read data from the input stream.
    /// Should be big enough to store a complete HTTP request from a client.
    /// </summary>
    public int ReceiveBuffer {
      get { return _SocketBuffersReceive; }
      set { _SocketBuffersReceive = value; }
    }

    /// <summary>
    /// The maximum number of threads in the thread pool. The number will get
    /// doubled for async I/O operation threads. If this value is too high,
    /// the server will run into performance problems due to excessive context
    /// switches. If there are many clients accessing your web server,
    /// you should increase this value to prevent congestion and to minimize
    /// response times. Increase this value, if there are "Congestion detected"
    /// entries in the logfile of the server.
    /// </summary>
    public int MaxThreads {
      get { return _SocketThreadsMax; }
      set { _SocketThreadsMax = value; }
    }

    /// <summary>
    /// Enable or disable the congestion control. If there are nearly no
    /// clients accessing your web server, disabling this can improve the 
    /// performance of the server slightly, but there will be the risk
    /// of a DoS attack on your server software.
    /// </summary>
    public bool CongestionPrevention {
      get { return _SocketCongestionPrevention; }
      set { _SocketCongestionPrevention = value; }
    }

    /// <summary>
    /// This value will be used to determine, whether the server's thread pool
    /// is currently congested or not. If the thread pool contains more than
    /// threads.max * limit worker threads, the server will accept no more
    /// keep-alive connections.
    /// </summary>
    public float CongestionLimit {
      get { return _SocketCongestionLimit; }
      set { _SocketCongestionLimit = value; }
    }

    /// <summary>
    /// The version of the HTTP protocol which should be used for this GenHTTP
    /// webserver. HTTP/1.1 supports pipelining and other techniques to speed
    /// up the data transfer to the client. Use HTTP/1.0 for compatibility
    /// reasons.
    /// </summary>
    /// <remarks>
    /// Note: if you set this value to 1.1 and a client connects using the HTTP 1.0
    /// protocol, the server will send a HTTP 1.0 response anyways.
    /// </remarks>
    public ProtocolType Protocol {
      get { return _HttpProtocolVersion; }
      set { _HttpProtocolVersion = value; }
    }

    /// <summary>
    /// Specifiy, whether the server should compress data to send or not.
    /// </summary>
    public bool CompressionEnabled {
      get { return _HttpCompressionEnabled; }
      set { _HttpCompressionEnabled = value; }
    }

    /// <summary>
    /// The algorithm used for compression.
    /// </summary>
    /// <remarks>
    /// Actually, there is only "gzip" you can choose.
    /// </remarks>
    public string CompressionAlgorithm {
      get { return _HttpCompressionAlgorithm; }
      set { _HttpCompressionAlgorithm = value; }
    }

    /// <summary>
    /// Responses which are smaller than this limit, will get compressed.
    /// This limit is necessary because gzip needs all the content available
    /// in RAM to compress it.
    /// </summary>
    public uint CompressionLimit {
      get { return _HttpCompressionLimit; }
      set { _HttpCompressionLimit = value; }
    }

    /// <summary>
    /// Set this attribute to "false", if you want to disable vhosts.
    /// </summary>
    public bool VHostsEnabled {
      get { return _HttpVHostsEnabled; }
      set { _HttpVHostsEnabled = value; }
    }

    /// <summary>
    /// The path to the project repository root.
    /// </summary>
    public string DocumentRoot {
      get { return _DocumentsRoot; }
      set { _DocumentsRoot = value; }
    }

    /// <summary>
    /// The path to the library containing the server helper to use.
    /// </summary>
    public string ServerHelper {
      get { return _DocumentsHelper; }
      set { _DocumentsHelper = value; }
    }

    /// <summary>
    /// The name of the class to instantiate in the assembly.
    /// </summary>
    public string ServerHelperType {
      get { return _DocumentsHelperType; }
      set { _DocumentsHelperType = value; }
    }

    /// <summary>
    /// Specify, whether the server should log information to the
    /// console or not. Enable this for debugging.
    /// Disabling the logging to the console will make the server
    /// software up to 5 times faster.
    /// </summary>
    public bool LogToConsole {
      get { return _LoggingConsole; }
      set { _LoggingConsole = value; }
    }

    /// <summary>
    /// Specify, whether the remoting interface should be available
    /// or not.
    /// </summary>
    public bool RemotingEnabled {
      get { return _RemotingEnabled; }
      set { _RemotingEnabled = value; }
    }

    /// <summary>
    /// Define the port which should be used for the TCP channel.
    /// </summary>
    public ushort RemotingPort {
      get { return _RemotingPort; }
      set { _RemotingPort = value; }
    }

    /// <summary>
    /// The hashed password required to manage the server remotely.
    /// </summary>
    public string RemotingPassword {
      get { return _RemotingPassword; }
      set { _RemotingPassword = value; }
    }

    /// <summary>
    /// Define, how much regular expressions should be cached.
    /// </summary>
    public ushort StaticRegexCacheSize {
      get { return _MiscRegexSize; }
      set { _MiscRegexSize = value; }
    }

    /// <summary>
    /// Defines the intervall of the server's timer. The server will
    /// fire the timer event in this intervall.
    /// </summary>
    public ushort TimerIntervall {
      get { return _MiscTimerIntervall; }
      set { _MiscTimerIntervall = value; }
    }

    #endregion

    #region Collections

    /// <summary>
    /// Add a rewrite directive to the server's configuration.
    /// </summary>
    /// <param name="config">The rewrite directive to add</param>
    public void AddRewrite(RewriteConfiguration config) {
      if (!_HttpRewrites.Contains(config)) { 
        _HttpRewrites.Add(config);
        GenerateCollectionFromLists();
      }
    }

    /// <summary>
    /// Remove a rewrite directive from the server's configuration.
    /// </summary>
    /// <param name="config">The rewrite directive to remove</param>
    public void RemoveRewrite(RewriteConfiguration config) {
      if (_HttpRewrites.Contains(config)) {
        _HttpRewrites.Remove(config);
        GenerateCollectionFromLists();
      }
    }

    /// <summary>
    /// Retrieve all rewrite directives.
    /// </summary>
    /// <returns>All available rewrite directives</returns>
    public Collection<RewriteConfiguration> GetRewrites() {
      return _Rewrites;
    }

    /// <summary>
    /// Add a vhost entry to the configuration.
    /// </summary>
    /// <param name="host">The host to handle</param>
    /// <param name="project">The project to redirect the client to</param>
    public void AddVHost(string host, string project) {
      if (!_HttpVHosts.ContainsKey(host)) {
        _HttpVHosts.Add(host, project);
        GenerateCollectionFromLists();
      }
    }

    /// <summary>
    /// Remove a vhost entry from the configuration.
    /// </summary>
    /// <param name="host">The virtual host to remove</param>
    public void RemoveVHost(string host) {
      if (_HttpVHosts.ContainsKey(host)) {
        _HttpVHosts.Remove(host);
        GenerateCollectionFromLists();
      }
    }

    /// <summary>
    /// Retrieve all available virtual hosts.
    /// </summary>
    /// <returns>A list with available virtual hosts</returns>
    public Collection<KeyValuePair<string, string>> GetVHosts() {
      return _VHosts;
    }

    #endregion

  }

}
