using GenHTTP.Engine.Infrastructure;

namespace GenHTTP.Engine.Protocol.Parser;

/// <summary>
/// Efficiently reads the body from the HTTP request, storing it
/// in a temporary file if it exceeds the buffering limits.
/// </summary>
internal sealed class RequestContentParser
{

    #region Get-/Setters

    internal long Length { get; }

    internal NetworkConfiguration Configuration { get; }

    #endregion

    #region Initialization

    internal RequestContentParser(long length, NetworkConfiguration configuration)
    {
            Length = length;
            Configuration = configuration;
        }

    #endregion

    #region Functionality

    internal async Task<Stream> GetBody(RequestBuffer buffer)
    {
            var body = Length > Configuration.RequestMemoryLimit ? TemporaryFileStream.Create() : new MemoryStream((int)Length);

            await CopyAsync(buffer, body, Length, Configuration.TransferBufferSize);

            body.Seek(0, SeekOrigin.Begin);

            return body;
        }

    internal static async ValueTask CopyAsync(RequestBuffer source, Stream target, long length, uint bufferSize)
    {
            var toFetch = length;

            while (toFetch > 0)
            {
                await source.ReadAsync();

                var toRead = Math.Min(source.Data.Length, Math.Min(bufferSize, toFetch));

                if (toRead == 0)
                {
                    throw new InvalidOperationException($"No data read from the transport but {toFetch} bytes are remaining");
                }

                var data = source.Data.Slice(0, toRead);

                var position = data.GetPosition(0);

                while (data.TryGet(ref position, out var memory))
                {
                    await target.WriteAsync(memory);
                }

                source.Advance(toRead);

                toFetch -= toRead;
            }
        }

    #endregion

}
