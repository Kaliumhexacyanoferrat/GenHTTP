using fdout;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Files.Handlers;

public class FileAssetsHandler : IHandler
{
    private readonly DirectoryInfo _directory;

    private readonly RandomAccessCache _cache;

    public FileAssetsHandler(DirectoryInfo directory)
    {
        _directory = directory;

        _cache = new RandomAccessCache(_directory.FullName);
    }

    public ValueTask PrepareAsync() => default;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        // todo: make this non-allocating
        var path = request.Header.Target.AsString(decode: true, remainingOnly: true);

        var target = Path.Combine(_directory.FullName, path);

        if (_cache.TryGet(target, out var entry))
        {
            var response = request.Respond()
                                  .Content(new EntryResponseContent(_cache, entry))
                                  .Build();

            return new(response);
        }

        return default;
    }

}
