using GenHTTP.Api.Protocol;
using GenHTTP.Core.Infrastructure.Configuration;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Core.Protocol
{

    internal class RequestParser
    {

        #region Get-/Setters

        private NetworkConfiguration Configuration { get; }

        #endregion

        #region Initialization

        internal RequestParser(NetworkConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Functionality

        internal async Task<RequestBuilder?> TryParseAsync(RequestBuffer buffer)
        {
            var request = new RequestBuilder();

            string? method, path, protocol;

            if ((method = await TryReadToken(buffer, ' ')) != null)
            {
                request.Type(method);
            }
            else { return null; }

            if ((path = await TryReadToken(buffer, ' ')) != null)
            {
                request.Path(path);
            }
            else { return null; }

            if ((protocol = await TryReadToken(buffer, '\r')) != null)
            {
                if (protocol.StartsWith("HTTP/"))
                {
                    request.Protocol(protocol.Substring(5));
                }
                else
                {
                    throw new ProtocolException($"Unrecognized HTTP protocol '{protocol}'");
                }
            }
            else { return null; }

            while (await TryReadHeader(buffer, request)) { /* nop */ }

            if (await TryReadToken(buffer, '\r') == null)
            {
                return null;
            }

            if (request.Headers.TryGetValue("Content-Length", out var bodyLength))
            {
                if (long.TryParse(bodyLength, out var length))
                {
                    if (length > 0)
                    {
                        var parser = new RequestContentParser(length, Configuration);

                        request.Content(await parser.GetBody(buffer));
                    }
                }
                else
                {
                    throw new ProtocolException("Content-Length header is expected to be a numeric value");
                }
            }
            
            return request;
        }

        private async Task<bool> TryReadHeader(RequestBuffer buffer, RequestBuilder request)
        {
            string? key, value;

            if ((key = await TryReadToken(buffer, ':')) != null)
            {
                if ((value = await TryReadToken(buffer, '\r')) != null)
                {
                    request.Header(key, value);
                    return true;
                }
            }

            return false;
        }

        private async Task<string?> TryReadToken(RequestBuffer buffer, char delimiter)
        {
            await buffer.Read();

            var position = buffer.Data.PositionOf((byte)delimiter);

            if (position == null)
            {
                await buffer.Read(true);
                position = buffer.Data.PositionOf((byte)delimiter);
            }

            if (position != null)
            {
                if (delimiter != '\r')
                {
                    var lineBreakPosition = buffer.Data.PositionOf((byte)'\r');

                    if (lineBreakPosition != null)
                    {
                        if (position.Value.GetInteger() > lineBreakPosition.Value.GetInteger())
                        {
                            return null;
                        }
                    }
                }

                var data = GetString(buffer.Data.Slice(0, position.Value));
                buffer.Advance(buffer.Data.GetPosition(1, position.Value));

                if (delimiter == '\r')
                {
                    buffer.Advance(1);
                }

                return data;
            }

            return null;
        }

        private string GetString(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return Encoding.ASCII.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.ASCII.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }

        #endregion

    }

}
