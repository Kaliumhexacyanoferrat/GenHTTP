using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

var service = Inline.Create().Get(() => "Hello World!");

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
    public MyData Hello() => new("Hello World!");

}
<<<<<<< HEAD
>>>>>>> 9f59196e (Add support for compiled service methods)
=======

public record MyData(string Data);
>>>>>>> 2e9361a1 (WIP)
