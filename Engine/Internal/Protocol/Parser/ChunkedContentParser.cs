using System.Buffers;
using System.Globalization;
using GenHTTP.Engine.Internal.Protocol.Parser.Conversion;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Protocol.Parser;

/// <summary>
/// Reads the chunked encoded body of a client request into
/// a stream.
/// </summary>
/// <remarks>
/// As we cannot know the length of the request beforehand,
/// this will always use a file stream for buffering.
/// </remarks>
internal sealed class ChunkedContentParser
{

    #region Initialization

    internal ChunkedContentParser(NetworkConfiguration networkConfiguration)
    {
        Configuration = networkConfiguration;
    }

    #endregion

    #region Get-/Setters

    private NetworkConfiguration Configuration { get; }

    #endregion

    #region Functionality

    internal async ValueTask<Stream> GetBody(RequestBuffer buffer)
    {
        var body = TemporaryFileStream.Create();

        var bufferSize = Configuration.TransferBufferSize;

        while (await NextChunkAsync(buffer, body, bufferSize)) { }

        body.Seek(0, SeekOrigin.Begin);

        return body;
    }

    private static async ValueTask<bool> NextChunkAsync(RequestBuffer buffer, Stream target, uint bufferSize)
    {
        await EnsureDataAsync(buffer);

        //
        // chunks are of the following form:
        //
        // ABC<CR><LF>
        // <DATA>
        // <CR><LF>
        //
        // with the final chunk having a size of 0
        //
        var chunkSize = GetChunkSize(buffer);

        if (chunkSize == 0)
        {
            return false;
        }
        await RequestContentParser.CopyAsync(buffer, target, chunkSize, bufferSize);

        buffer.Advance(2);

        return true;
    }

    private static long GetChunkSize(RequestBuffer buffer)
    {
        var reader = new SequenceReader<byte>(buffer.Data);

        if (reader.IsNext((byte)'0'))
        {
            buffer.Advance(5);
            return 0;
        }
        if (reader.TryReadTo(out ReadOnlySequence<byte> lengthInHex, (byte)'\r'))
        {
            var hexString = ValueConverter.GetString(lengthInHex);

            var length = long.Parse(hexString, NumberStyles.HexNumber);

            buffer.Advance(lengthInHex.Length + 2);

            return length;
        }
        throw new ProtocolException("Chunk size expected");
    }

    private static async ValueTask EnsureDataAsync(RequestBuffer buffer)
    {
        if (buffer.ReadRequired)
        {
            if (await buffer.ReadAsync() == null)
            {
                throw new ProtocolException("Timeout while waiting for client data");
            }
        }
    }

    #endregion

}
