using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var content = Inline.Create().Get(() => "Hello World!");

await Host.Create()
    .Handler(content)
    .Defaults()
    .RunAsync(); // or StartAsync() for non-blocking