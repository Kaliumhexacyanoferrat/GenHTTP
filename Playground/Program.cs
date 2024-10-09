using GenHTTP.Engine;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;

// Content.From(Resource.FromString("Hello World"))

Host.Create()
    .Handler(Listing.From(ResourceTree.FromDirectory("C:/")))
    .Defaults()
    .Development()
    .Console()
    .Run();

