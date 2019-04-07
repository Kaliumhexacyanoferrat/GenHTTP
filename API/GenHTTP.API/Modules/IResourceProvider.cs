using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Api.Modules
{

    /// <summary>
    /// Allows content providers to access a resource without the need
    /// to know the actual way of access (such as database, web, or
    /// file system).
    /// </summary>
    /// <remarks>
    /// As resources may change (e.g. if an user changes the file that
    /// is provided as a resource), content providers must not
    /// cache the results of a method call.
    /// </remarks>
    public interface IResourceProvider
    {

        /// <summary>
        /// Returns the read-only stream of the resource to be accessed.
        /// </summary>
        /// <returns>The resource to be accessed</returns>
        Stream GetResource();
        
    }

}
