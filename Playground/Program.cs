using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Layouting;

var api = Inline.Create()
                .Get(() => 42);

var app = Layout.Create()
                .Add(api)
                .AddOpenApi()
                .AddSwaggerUI()
                .AddRedoc();

// var app = Content.From(Resource.FromString("Hello World"));

await Host.Create()
          .Handler(app)
          .Defaults()
          .Development()
          .Console()
          .RunAsync();
