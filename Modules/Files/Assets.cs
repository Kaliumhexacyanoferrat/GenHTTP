using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Files.Handlers;

namespace GenHTTP.Modules.Files;

public static class Assets
{

    public static TreeAssetsBuilder From(IBuilder<IResourceTree> tree) => new(tree.Build());

    public static TreeAssetsBuilder From(IResourceTree tree) => new(tree);

    public static FileAssetsBuilder From(string directory) => new(new(directory));

    public static FileAssetsBuilder From(DirectoryInfo directory) => new(directory);

}
