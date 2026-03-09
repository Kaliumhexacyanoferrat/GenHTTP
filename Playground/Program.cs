using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Playground;

var app = Layout.Create()
                .Add("plaintext", Content.From(Resource.FromString("Hello World!")))
                .Add("json", new FixedJsonHandler())
                .Add("jsonc", new ChunkedJsonHandler());

await Host.Create()
          .Handler(app)
          // .Defaults()
          .Console()
          .RunAsync();
