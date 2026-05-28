using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities;

public class NonSeekableResource(IResource source) : IResource
{

    public string? Name => null;

    public DateTime? Modified => null;

    public ContentType? ContentType => null;

    public ulong? Length => null;

    public ValueTask<ulong> CalculateChecksumAsync() => new(0);

    public async ValueTask<Stream> GetContentAsync() => new NonSeekableStream(await source.GetContentAsync());

    public ValueTask WriteAsync(IResponseSink sink) => ((IResource)this).WriteAsync(sink);

}
