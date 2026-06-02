using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Files.Handlers;

public sealed class FileAssetsBuilder(DirectoryInfo directory) : IHandlerBuilder<FileAssetsBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<ICompressionAlgorithm> _algorithms = [];

    private char _separator = '.';

    public FileAssetsBuilder AllowPrecompressed(params ICompressionAlgorithm[] algorithms)
    {
        _algorithms.AddRange(algorithms);
        return this;
    }

    public FileAssetsBuilder AllowPrecompressed(ICompressionAlgorithm[] algorithms, char separator)
    {
        _separator = separator;

        return AllowPrecompressed(algorithms);
    }

    public FileAssetsBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new FileAssetsHandler(directory, _algorithms, _separator));
    }

}
