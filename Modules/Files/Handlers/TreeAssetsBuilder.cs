using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Files.Handlers;

public sealed class TreeAssetsBuilder(IResourceTree tree) : IHandlerBuilder<TreeAssetsBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<ICompressionAlgorithm> _algorithms = [];

    private char _separator = '.';

    public TreeAssetsBuilder AllowPrecompressed(params ICompressionAlgorithm[] algorithms)
    {
        _algorithms.AddRange(algorithms);
        return this;
    }

    public TreeAssetsBuilder AllowPrecompressed(ICompressionAlgorithm[] algorithms, char separator)
    {
        _separator = separator;

        return AllowPrecompressed(algorithms);
    }

    public TreeAssetsBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new TreeAssetsHandler(tree, _algorithms, _separator));
    }

}
