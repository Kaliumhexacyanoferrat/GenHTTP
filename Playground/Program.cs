using GenHTTP.Engine.Internal;
using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;
using Microsoft.Extensions.DependencyInjection;

var app = Layout.Create()
                .AddService<MyWebservice>("service");

var services = new ServiceCollection();

services.AddSingleton<MyService>()
        .AddSingleton<MyOtherService>();

var provider = services.BuildServiceProvider();

await Host.Create()
          .AddDependencyInjection(provider)
          .Handler(app)
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
