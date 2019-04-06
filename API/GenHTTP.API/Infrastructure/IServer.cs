using System;
using System.Threading.Tasks;

using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    public interface IServer : IDisposable
    {

        Version Version { get; }

        bool Development { get; }

        IServerCompanion? Companion { get; }

        IExtensionCollection Extensions { get; }

        IRouter Router { get; }
                  
    }

}
