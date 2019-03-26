using System;

using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    public interface IServer : IDisposable
    {

        Version Version { get; }

        IServerCompanion? Companion { get; }

        IRouter Router { get; }
          
    }

}
