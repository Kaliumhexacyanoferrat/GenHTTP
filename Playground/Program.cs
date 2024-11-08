using GenHTTP.Engine.Kestrel;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var app = Listing.From(ResourceTree.FromDirectory("."));

await Host.Create()
          .Handler(app)
          .Defaults()
          .Development()
          .Console()
          .RunAsync();
