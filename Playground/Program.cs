using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;

var app = Content.From(Resource.FromString("Hello World!"));

await Host.Create()
          .Handler(app)
          // .Defaults()
          .Console()
          .RunAsync();
