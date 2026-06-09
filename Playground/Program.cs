using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

//var app = Layout.Create()
//                .Add("plaintext", Content.From(Resource.FromString("Hello World!")));

var app = Inline.Create()
                .Get((int a, int b) => a + b);

await Host.Create()
          .Handler(app)
          .Console()
          .RunAsync();
