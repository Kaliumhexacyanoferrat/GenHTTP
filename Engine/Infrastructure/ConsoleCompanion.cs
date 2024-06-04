using System;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Infrastructure
{

    internal sealed class ConsoleCompanion : IServerCompanion
    {

        public void OnRequestHandled(IRequest request, IResponse response)
        {
            Console.WriteLine($"REQ - {request.Client.IPAddress} - {request.Method.RawMethod} {request.Target.Path} - {response.Status.RawStatus} - {response.ContentLength ?? 0}");
        }

        public void OnServerError(ServerErrorScope scope, IPAddress? client, Exception error)
        {
            Console.WriteLine($"ERR - {client : 'n/a'} - {scope} - {error}");
        }

    }

}
