using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Provides extensions available in a certain scope, such as the
    /// extensions registered at the server.
    /// </summary>
    public interface IExtensionCollection : IReadOnlyList<IServerExtension>
    {

    }

}
