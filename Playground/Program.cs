using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

var service = ServiceResource.From(new MyService());

await Host.Create()
<<<<<<< HEAD
          .Handler(content)
          .Defaults()
          .Console()
          .RunAsync(); // or StartAsync() for non-blocking
=======
    .Handler(service)
    .Defaults()
    .RunAsync(); // or StartAsync() for non-blocking

public class MyService
{

    [ResourceMethod]
    public string Hello() => "Hello World!";

}
>>>>>>> 9f59196e (Add support for compiled service methods)
