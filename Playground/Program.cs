using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Archives;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var archive = Resource.FromWeb("https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.102/dotnet-sdk-10.0.102-linux-x64.tar.gz");

var tree = ArchiveTree.From(archive);

var listing = Listing.From(tree);

await Host.Create()
          .Handler(listing)
          .Defaults()
          .Console()
          .RunAsync();
