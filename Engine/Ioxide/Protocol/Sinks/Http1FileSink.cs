using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Ioxide;
using GenHTTP.Api.Protocol;

using IoConnection = ioxide.TcpConnection;

namespace GenHTTP.Engine.Ioxide.Protocol.Sinks;

/// <summary>
/// <see cref="Http1Sink"/> on a connection whose writer is its own write buffer, in cleartext or with kernel TLS: a
/// file is read off the ring straight into that buffer, after the headers, so its bytes are never copied.
/// </summary>
internal sealed class Http1FileSink(PipeWriter writer, IoConnection connection) : IResponseSink, IIoxideFileSink
{
    // ReadFileAsync grows the write buffer to whatever it is asked for, so each read stays well under its default
    // 16 KiB, leaving room for the status line and headers ahead of the first.
    private const int Chunk = 12 * 1024;

    private Stream? _stream;

    public IBufferWriter<byte> Writer => writer;

    public Stream Stream => _stream ??= new Http1WriterStream(writer, writer);

    public async ValueTask WriteFileAsync(int fd, long offset, long length)
    {
        var end = offset + length;

        while (offset < end)
        {
            var read = await connection.ReadFileAsync(fd, (int)Math.Min(end - offset, Chunk), offset);

            if (read <= 0)
            {
                break;
            }

            connection.AdvanceWrite(read);
            offset += read;

            if ((await writer.FlushAsync()).IsCompleted)
            {
                throw new ConnectionClosedException();
            }
        }
    }
}
