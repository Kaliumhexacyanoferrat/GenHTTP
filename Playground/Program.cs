using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var content = Content.From(Resource.FromString("Hello World!"));

await Host.Create()
          .Handler(content)
          // .Defaults()
          .RunAsync(); // or StartAsync() for non-blocking
