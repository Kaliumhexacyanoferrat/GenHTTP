using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

var app = Layout.Create()
                .Add("plaintext", Content.From(Resource.FromString("Hello World!")));

await Host.Create()
          .Handler(app)
          // .Defaults()
          .Console()
          .RunAsync();
