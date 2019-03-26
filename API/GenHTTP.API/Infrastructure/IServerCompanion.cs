using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Infrastructure
{

    public enum ServerErrorScope
    {
        ServerConnection,
        ClientConnection,
        Parser,
        PageGeneration,
        Internal
    }

    public interface IServerCompanion
    {

        void OnRequestHandled(IHttpRequest request, IHttpResponse response, Exception? error);

        void OnServerError(ServerErrorScope scope, Exception error);

    }

}
