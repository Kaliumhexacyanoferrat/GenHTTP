using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.ReverseProxy;
using GenHTTP.Modules.Webservices;

var content = Content.From(Resource.FromString("Hello World!"));

var layout = Layout.Create()
    .AddService<Service>("/").Build();

var proxy = Proxy
    .Create()
    .Upstream("ws://localhost:5000/");

await Host.Create()
          .Port(8080)
          .Handler(proxy)
          .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking

public class Service
{
    [ResourceMethod]
    public IResponse Endpoint(IRequest request)
    {
        return request
            .Respond()
            .Type("application/json")
            .Build();
    }
}