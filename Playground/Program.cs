using GenHTTP.Engine.Internal;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Playground;

var app = Layout.Create()
                .Add("plaintext", Content.From(Resource.FromString("Hello World!")))
                .Add("jsonf", new FixedJsonHandler())
                .Add("json", new ChunkedJsonHandler());

await Host.Create()
          .Handler(app)
          // .Defaults()
          .Console()
          .RunAsync();
