using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.LoadBalancing;

namespace GenHTTP.Playground.Samples;

public static class LoadBalancerSample
{

    private static readonly ByteString CanaryHeader = new("X-Canary");

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Distributes API requests over multiple worker processes (expected to run
         * on http://localhost:5001 and :5002) and sends clients downloading files
         * to one of the mirror servers.
         *
         * Clients sending an "X-Canary" header are served by a canary worker
         * (on http://localhost:5003), so a new version can be tested with real traffic.
         *
         * See https://genhttp.org/documentation/content/handlers/load-balancer/
         *
         */

        var workers = LoadBalancer.Create()
                                  .Proxy("http://localhost:5001/")
                                  .Proxy("http://localhost:5002/")
                                  .Proxy("http://localhost:5003/", r => r.Header.Headers.ContainsKey(CanaryHeader) ? Priority.High : Priority.Low);

        var mirrors = LoadBalancer.Create()
                                  .Redirect("https://mirror1.example.com/")
                                  .Redirect("https://mirror2.example.com/");

        return Layout.Create()
                     .Add("api", workers) // GET http://localhost:8080/load-balancing/api/
                     .Add("downloads", mirrors); // GET http://localhost:8080/load-balancing/downloads/setup.exe
    }

}
