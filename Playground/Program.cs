using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var app = Content.From(Resource.FromString("Hello World"));

await Host.Create()
          .Handler(app)
          .Defaults()
          .Development()
          .Console()
          .RunAsync();
