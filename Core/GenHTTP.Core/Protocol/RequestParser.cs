using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Protocol
{

    internal class RequestParser
    {
        private RequestBuilder? _Builder;

        #region Get-/Setters

        private NetworkConfiguration Configuration { get; }

        private RequestBuilder Request => _Builder ?? (_Builder = new RequestBuilder());

        #endregion

        #region Initialization

        internal RequestParser(NetworkConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Functionality

        internal async ValueTask<RequestBuilder?> TryParseAsync(RequestBuffer buffer)
        {
            string? method, path, protocol;

            if ((method = await TryReadToken(buffer, ' ').ConfigureAwait(false)) != null)
            {
                Request.Type(method);
            }
            else
            {
                return null;
            }

            if ((path = await TryReadToken(buffer, ' ').ConfigureAwait(false)) != null)
            {
                Request.Path(path);
            }
            else
            {
                return null;
            }

            if ((protocol = await TryReadToken(buffer, '\r', 1, 5).ConfigureAwait(false)) != null)
            {
                Request.Protocol(protocol);
            }
            else
            {
                return null;
            }

            while (await TryReadHeader(buffer, Request).ConfigureAwait(false)) { /* nop */ }

            if ((await TryReadToken(buffer, '\r', 1).ConfigureAwait(false)) == null)
            {
                return null;
            }

            if (Request.Headers.TryGetValue("Content-Length", out var bodyLength))
            {
                if (long.TryParse(bodyLength, out var length))
                {
                    if (length > 0)
                    {
                        var parser = new RequestContentParser(length, Configuration);

                        Request.Content(await parser.GetBody(buffer).ConfigureAwait(false));
                    }
                }
                else
                {
                    throw new ProtocolException("Content-Length header is expected to be a numeric value");
                }
            }

            var result = Request;
            _Builder = null;

            return result;
        }

        private async ValueTask<bool> TryReadHeader(RequestBuffer buffer, RequestBuilder request)
        {
            string? key, value;

            if ((key = await TryReadToken(buffer, ':', 1).ConfigureAwait(false)) != null)
            {
                if ((value = await TryReadToken(buffer, '\r', 1).ConfigureAwait(false)) != null)
                {
                    request.Header(key, value);
                    return true;
                }
            }

            return false;
        }

        private async ValueTask<string?> TryReadToken(RequestBuffer buffer, char delimiter, ushort skipNext = 0, ushort skipFirst = 0)
        {
            if (buffer.ReadRequired)
            {
                if ((await buffer.Read().ConfigureAwait(false)) == null)
                {
                    return null;
                }
            }

            var position = buffer.Data.PositionOf((byte)delimiter);

            if (position == null)
            {
                if ((await buffer.Read(true).ConfigureAwait(false)) == null)
                {
                    return null;
                }

                position = buffer.Data.PositionOf((byte)delimiter);
            }

            if (position != null)
            {
                if (skipFirst > 0)
                {
                    buffer.Advance(skipFirst);
                    position = buffer.Data.PositionOf((byte)delimiter)!;
                }

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

                buffer.Advance(skipNext);

                return data;
            }

            return null;
        }

        private string GetString(ReadOnlySequence<byte> buffer)
        {
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
