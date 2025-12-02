using GenHTTP.Api.Routing;
using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class BenchmarkContext
{

    public Request Request { get; init; }

    public Response Response { get; init; }

    public static BenchmarkContext Create()
    {
        var response = new Response();

        var builder = new ResponseBuilder(response);

        var request = new Request(builder);

        return new BenchmarkContext(request, response);
    }

    private BenchmarkContext(Request request, Response response)
    {
        Request = request;
        Response = response;
    }

    public void Reset()
    {
        Request.Target = new RoutingTarget(Request.Target.Path);

        if (Request.Content?.CanSeek ?? false)
        {
            Request.Content.Seek(0, SeekOrigin.Begin);
        }

        Response.Reset();
    }

}
