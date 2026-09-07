using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Files.Multi;

public sealed class FileAssetsHandler : IHandler
{
    private readonly DirectoryInfo _directory;

    private readonly List<ICompressionAlgorithm> _algorithms;

    private readonly char _separator;

    private readonly TimeSpan _refreshInterval;

    private IHandler? _inner;

    public FileAssetsHandler(DirectoryInfo directory, List<ICompressionAlgorithm> algorithms, char separator, TimeSpan refreshInterval)
    {
        _directory = directory;
        _algorithms = algorithms;
        _separator = separator;
        _refreshInterval = refreshInterval;
    }

    public ValueTask PrepareAsync(IServer server)
    {
        _inner = server.ServerEngine == ServerEngine.Ioxide
            ? new IoxideFilesHandler(_directory.FullName, _refreshInterval, _algorithms, _separator)
            : new RegularFileAssetHandler(_directory, _algorithms, _separator);

        return _inner.PrepareAsync(server);
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => _inner!.HandleAsync(request);

}
