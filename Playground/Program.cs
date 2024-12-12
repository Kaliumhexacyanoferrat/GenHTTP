using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Swagger;

var app = Layout.Create()
                .Add(Inline.Create().Get(() => "Hello World"))
                .AddOpenApi()
                .AddSwaggerUI();

await Host.Create()
          .Handler(app)
          .Defaults()
          .Development()
          .Console()
          .RunAsync();
