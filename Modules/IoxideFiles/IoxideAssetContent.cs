using System.Buffers;

using Microsoft.Win32.SafeHandles;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Ioxide;

using ioxide;
using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Writes one asset's body to the response sink, flush-disciplined so it never stages more than
/// <see cref="Chunk"/> bytes into the ioxide write slab at once. Re-resolves the asset under its own
/// lease, so nothing is held across an await.
/// </summary>
public sealed class IoxideAssetContent(StaticAssets assets, string path, long length, ContentType contentType, ReadOnlyMemory<byte>? contentEncoding) : IResponseContent
{
    // Stay well under the default 16 KB slab, leaving room for the status + headers GenHTTP already
    // staged before the body (the engine does not flush between them).
    private const int Chunk = 12 * 1024;

    public ulong? Length => (ulong)length;

    public ContentType? Type => contentType;

    public ReadOnlyMemory<byte>? Encoding => contentEncoding;

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong?)null);

    public async ValueTask WriteAsync(IResponseSink sink)
    {
        using var lease = assets.Acquire();

        if (!lease.TryGet(path, out var asset))
        {
            return; // vanished between header and body (rare)
        }

        if (AssetCache.IsFresh(asset, out var exists, out _))
        {
            if (asset.Response != 0)
            {
                // Fresh + baked: write just the body (GenHTTP framed the header). The baked block is
                // header+body in native memory; the body is the trailing asset.Length bytes.
                await WriteNative(sink, asset.Response + (nint)(asset.ResponseLength - asset.Length), asset.Length);
            }
            else
            {
                // Fresh but too large to bake: read off the ring from the cached fd.
                await WriteFromDisk(sink, asset.Fd, length);
            }
        }
        else if (exists)
        {
            // Changed on disk (edit or atomic rename): open the current path fresh so a rename resolves
            // to the new inode, not the cached fd.
            await WriteChanged(sink, asset.Path, length);
        }
    }

    // Copy native memory to the sink in flushed <= Chunk slices; FlushAsync drains the slab to the socket.
    private static async ValueTask WriteNative(IResponseSink sink, nint data, long len)
    {
        long sent = 0;

        while (sent < len)
        {
            var n = (int)Math.Min(len - sent, Chunk);
            WriteChunk(sink.Writer, data + (nint)sent, n);
            await sink.Stream.FlushAsync();
            sent += n;
        }
    }

    // The one unsafe step - stage a native slice into the writer's buffer. Kept non-async so it never
    // sits across an await (you cannot await in an unsafe context).
    private static unsafe void WriteChunk(IBufferWriter<byte> writer, nint data, int len)
        => writer.Write(new ReadOnlySpan<byte>((byte*)data, len));

    private static async ValueTask WriteFromDisk(IResponseSink sink, int fd, long len)
    {
        var readers = RentPool();
        var reader = await readers.RentAsync();

        try
        {
            long offset = 0;

            while (offset < len)
            {
                var read = await reader.ReadAsync(fd, offset);
                if (read <= 0)
                {
                    break;
                }

                var take = (int)Math.Min(read, len - offset);
                await WriteNative(sink, reader.Buffer, take);
                offset += take;
            }
        }
        finally
        {
            readers.Return(reader);
        }
    }

    private static async ValueTask WriteChanged(IResponseSink sink, string filePath, long len)
    {
        SafeFileHandle handle;

        try
        {
            handle = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch
        {
            return; // raced with a delete
        }

        try
        {
            await WriteFromDisk(sink, (int)handle.DangerousGetHandle(), len);
        }
        finally
        {
            handle.Dispose();
        }
    }

    // The AssetReader pool is per-reactor; ioxide's GetService throws if absent, so create-and-self-
    // register on first use on this reactor.
    private static RingPool<AssetReader> RentPool()
    {
        var reactor = IoxideReactor.Current;

        try
        {
            return reactor.GetService<RingPool<AssetReader>>();
        }
        catch (InvalidOperationException)
        {
            return AssetReader.CreatePool(reactor, readers: 4, bufferBytes: 1 << 20);
        }
    }
}
