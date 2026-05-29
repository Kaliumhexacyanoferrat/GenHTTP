using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Compression.PreCompression;

namespace GenHTTP.Modules.Compression;

public static class PreCompressedResources
{

    public static PreCompressedResourceHandlerBuilder From(IBuilder<IResourceTree> tree)
        => From(tree.Build());

    public static PreCompressedResourceHandlerBuilder From(IResourceTree tree)
        => new(tree);

}
