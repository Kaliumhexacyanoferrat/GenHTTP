using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    public interface IServerBuilder : IBuilder<IServer>
    {

        IServerBuilder Router(IRouterBuilder routerBuilder);

        IServerBuilder Router(IRouter router);

        IServerBuilder Companion(IServerCompanion companion);

        IServerBuilder Port(ushort port);

        IServerBuilder Backlog(ushort backlog);

    }
    
}
