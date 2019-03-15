using GenHTTP.Api.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{
    
    public interface IProjectCollection
    {

        /// <summary>
        /// Retrieve a project by name.
        /// </summary>
        /// <param name="name">The name of the project to retrieve</param>
        /// <returns>The requested project</returns>
        IProject this[string name] { get; }

        /// <summary>
        /// Retrieve the names of all available projects.
        /// </summary>
        string[] Projects { get; }

        /// <summary>
        /// Check whether a project exists or not.
        /// </summary>
        /// <param name="name">The name of the project to check for existance.</param>
        /// <returns>True, if the project exists on this server</returns>
        bool Exists(string name);

        /// <summary>
        /// Returns the enumerator of the project list.
        /// </summary>
        /// <returns>The enumerator of the project list</returns>
        IEnumerator<IProject> GetEnumerator();

        /// <summary>
        /// Unload the given project.
        /// </summary>
        /// <param name="project">The name of the project to unload</param>
        void Unload(string project);

        /// <summary>
        /// Load a project from an assembly.
        /// </summary>
        /// <param name="file">The file which contains the project</param>
        /// <param name="completeClassName">The type of the class to instantiate (e.h. MyNamespace.MyClass)</param>
        void Load(string file, string completeClassName);
        
    }

}
