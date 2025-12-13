using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Practices;

var content = Inline.Create()
                    .Get(() => "Hello World!")
                    .BuildAs("identifier");

await Host.Create()
    .Handler(content)
    .Defaults()
    .RunAsync(); // or StartAsync() for non-blocking