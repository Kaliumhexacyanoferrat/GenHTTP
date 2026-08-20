using GenHTTP.Engine.Internal;
using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;

// use a handler of your choice (see the samples below)
var api = Layout.Create()
                .Add(Inline.Create().Get(() => "Hello World"))
                .AddOpenApi()
                .AddScalar();

var host = await Host.Create()
                     .Handler(api)
                     .Defaults()
                     .StartAsync(); // or .RunAsync() to block until the (console) application is shut down
