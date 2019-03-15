using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace GenHTTP
{

    /// <summary>
    /// A collection with available projects.
    /// </summary>
    [Serializable]
    public class ProjectCollection : MarshalByRefObject
    {
        private Dictionary<string, IProject> _Projects;
        private Server _Server;

        /// <summary>
        /// Initialize the server's project list. Load the found projects.
        /// </summary>
        /// <param name="server">The server this project collection relates to</param>
        internal ProjectCollection(Server server)
        {
            _Projects = new Dictionary<string, IProject>();
            _Server = server;
            // search directory for projects
            string folder = server.Configuration.DocumentRoot;
            if (folder == "" || folder == null) folder = "./projects/";
            if (!Directory.Exists(folder))
            {
                server.Log.WriteTimestamp();
                server.Log.WriteLineColored("Documents root not found", ConsoleColor.Red);
                server.Log.WriteLine("");
                return;
            }
            DirectoryInfo d = new DirectoryInfo(folder);
            foreach (DirectoryInfo directory in d.GetDirectories())
            {
                if (directory.Name.StartsWith("_")) continue;
                foreach (FileInfo file in directory.GetFiles("*.dll"))
                {
                    try
                    {
                        // try to load this dll
                        Assembly targetAssembly = Assembly.LoadFile(file.FullName);
                        foreach (Type t in targetAssembly.GetTypes())
                        { // search for classes implementing the IPlugin-Interface
                            if (t.IsPublic && t.IsClass)
                            {
                                Type[] test = t.FindInterfaces(new TypeFilter(CheckInterface), typeof(IProject));
                                if (test.Length > 0)
                                { // this class implements our interface
                                    IProject proj = (IProject)targetAssembly.CreateInstance(t.Namespace + "." + t.Name);
                                    Log log = new Log(directory.FullName + @"/logs/" + Log.DateTimeString + ".log", false);
                                    proj.Init(server, directory.FullName, log);
                                    server.Log.WriteTimestamp();
                                    server.Log.WriteLineColored("Project '" + proj.Name + ", " + proj.Version + "' loaded", ConsoleColor.White);
                                    _Projects.Add(proj.Name, proj);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        server.Log.WriteTimestamp();
                        server.Log.WriteLineColored("Couldn't load project '" + directory.Name + @"/" + file.Name + "'", ConsoleColor.Red);
                        server.Log.WriteLine("");
                        server.Log.WriteLine(e.Message);
                        server.Log.WriteLine("");
                        server.Log.WriteLine(e.StackTrace);
                        server.Log.WriteLine("");
                    }
                }
            }
            server.Log.WriteLine("");
        }

        private bool CheckInterface(Type t, object o)
        {
            Type filter = (Type)o;
            return (t.Name == filter.Name && t.Namespace == filter.Namespace);
        }

        #region get-/setters

        /// <summary>
        /// Retrieve a project by name.
        /// </summary>
        /// <param name="name">The name of the project to retrieve</param>
        /// <returns>The requested project</returns>
        public IProject this[string name]
        {
            get
            {
                if (_Projects.ContainsKey(name)) return _Projects[name];
                throw new Exception("Project not found");
            }
        }

        /// <summary>
        /// Retrieve the names of all available projects.
        /// </summary>
        public string[] Projects
        {
            get
            {
                string[] retval = new string[_Projects.Keys.Count];
                _Projects.Keys.CopyTo(retval, 0);
                return retval;
            }
        }

        #endregion

        /// <summary>
        /// Check whether a project exists or not.
        /// </summary>
        /// <param name="name">The name of the project to check for existance.</param>
        /// <returns>True, if the project exists on this server</returns>
        public bool Exists(string name)
        {
            return _Projects.ContainsKey(name);
        }

        /// <summary>
        /// Returns the enumerator of the project list.
        /// </summary>
        /// <returns>The enumerator of the project list</returns>
        public IEnumerator<IProject> GetEnumerator()
        {
            return _Projects.Values.GetEnumerator();
        }

        /// <summary>
        /// Unload the given project.
        /// </summary>
        /// <param name="project">The name of the project to unload</param>
        public void Unload(string project)
        {
            if (_Projects.ContainsKey(project)) _Projects.Remove(project);
        }

        /// <summary>
        /// Load a project from an assembly.
        /// </summary>
        /// <param name="file">The file which contains the project</param>
        /// <param name="completeClassName">The type of the class to instantiate (e.h. MyNamespace.MyClass)</param>
        public void Load(string file, string completeClassName)
        {
            if (!File.Exists(file)) throw new FileNotFoundException();
            FileInfo info = new FileInfo(file);
            // try to load this dll
            Assembly targetAssembly = Assembly.LoadFile(file);
            // create an instance
            IProject proj = (IProject)targetAssembly.CreateInstance(completeClassName);
            if (proj == null) throw new Exception("The given type could not be found in the assembly");
            if (!_Projects.ContainsKey(proj.Name))
            {
                try
                {
                    // intialize the project
                    Log log = new Log(info.Directory.FullName + @"/logs/" + Log.DateTimeString + ".log", false);
                    proj.Init(_Server, info.Directory.FullName, log);
                    _Server.Log.WriteTimestamp();
                    _Server.Log.WriteLineColored("Project '" + proj.Name + ", " + proj.Version + "' loaded", ConsoleColor.White);
                    _Projects.Add(proj.Name, proj);
                }
                catch (Exception ex)
                {
                    _Server.Log.WriteTimestamp();
                    _Server.Log.WriteLineColored("Couldn't load project '" + file + "'", ConsoleColor.Red);
                    _Server.Log.WriteLine("");
                    _Server.Log.WriteLine(ex.Message);
                    _Server.Log.WriteLine("");
                    _Server.Log.WriteLine(ex.StackTrace);
                    _Server.Log.WriteLine("");
                    throw ex;
                }
            }
            else
            {
                throw new Exception("This project is already loaded");
            }
        }

    }

}
