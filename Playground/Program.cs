using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Internal;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.DependencyInjection;

var app = Layout.Create()
                .AddDependentService<MyWebservice>("service");

var services = new ServiceCollection();

services.AddSingleton<MyService>()
        .AddSingleton<MyOtherService>();

var provider = services.BuildServiceProvider();

await Host.Create()
          .AddDependencyInjection(provider)
          .Handler(app.Add(Dependent.Concern<MyConcern>()))
          .Defaults()
          .Development()
          .Console()
          .RunAsync();

public class MyService
{

    public int Generate() => 0815;

}

public class MyOtherService
{

    public int Generate() => 0711;

}

public class MyWebservice
{
    private MyService _Service;

    public MyWebservice(MyService service)
    {
        _Service = service;
    }

    [ResourceMethod]
    public int Get(MyOtherService other)
    {
        return _Service.Generate() + other.Generate();
    }

}

public class MyConcern : IDependentConcern
{
    private MyOtherService _Service;

    public MyConcern(MyOtherService service)
    {
        _Service = service;
    }

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        var response = await content.HandleAsync(request);

        if (response != null)
        {
            response.Headers.Add("X-Custom", _Service.Generate().ToString());
        }

        return response;
    }

}
