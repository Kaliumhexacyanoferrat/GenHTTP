using System.Buffers;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class ResourceContent : IResponseContent
{

    #region Initialization

    public ResourceContent(IResource resource)
    {
        Resource = resource;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => Resource.Length;

    private IResource Resource { get; }

    #endregion

    #region Functionality

    public async ValueTask<ulong?> CalculateChecksumAsync() => await Resource.CalculateChecksumAsync();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Resource.WriteAsync(target, bufferSize);

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        // todo: rework IResource infrastructure

        var writer = sink.Writer;

        var buffer = ArrayPool<byte>.Shared.Rent(4096);

        try
        {
            using var source = await Resource.GetContentAsync();

            int read;

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer.AsSpan().Slice(0, read));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

    }

    #endregion

}
