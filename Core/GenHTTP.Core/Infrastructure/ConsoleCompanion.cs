using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Infrastructure
{

    internal class ConsoleCompanion : IServerCompanion
    {

        public void OnRequestHandled(IRequest request, IResponse response)
        {
            Console.WriteLine($"REQ - {request.Client.IPAddress} - {request.Method.RawMethod} {request.Target.Path} - {response.Status.RawStatus} - {response.ContentLength ?? 0}");
        }

        public void OnServerError(ServerErrorScope scope, Exception error)
        {
            Console.WriteLine($"ERR - {scope} - {error}");
        }

    }

}
