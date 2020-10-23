using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Engine.Infrastructure.Configuration;

namespace GenHTTP.Engine.Protocol
{

    internal class RequestParser
    {
        private static char[] LINE_ENDING = new char[] { '\r' };
        private static char[] PATH_ENDING = new char[] { '\r', '\n', '?', ' ' };

        private static readonly Encoding ASCII = Encoding.ASCII;

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
            WebPath? path;
            RequestQuery? query;

            string? method, protocol;

            if ((method = await ReadToken(buffer, ' ').ConfigureAwait(false)) != null)
            {
                Request.Type(method);
            }
            else
            {
                return null;
            }

            if ((path = await TryReadPath(buffer).ConfigureAwait(false)) != null)
            {
                Request.Path(path);
            }
            else
            {
                return null;
            }

            if ((query = await TryReadQuery(buffer).ConfigureAwait(false)) != null)
            {
                Request.Query(query);
            }

            if ((protocol = await ReadToken(buffer, '\r', 1, 5).ConfigureAwait(false)) != null)
            {
                Request.Protocol(protocol);
            }
            else
            {
                return null;
            }

            while (await TryReadHeader(buffer, Request).ConfigureAwait(false)) { /* nop */ }

            if ((await ReadToken(buffer, '\r', 1).ConfigureAwait(false)) == null)
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

        private async ValueTask<WebPath?> TryReadPath(RequestBuffer buffer)
        {
            // ignore the slash at the beginning
            buffer.Advance(1);

            var parts = new List<string>(4);

            string? part;

            while ((part = await ReadToken(buffer, '/', PATH_ENDING).ConfigureAwait(false)) != null)
            {
                parts.Add(Uri.UnescapeDataString(part));
            }

            // find the delimiter (either '?' or ' ')
            var remainder = await ReadToken(buffer, '?') ?? await ReadToken(buffer, ' ');

            if (remainder != null)
            {
                if (string.IsNullOrEmpty(remainder))
                {
                    return new WebPath(parts, true);
                }

                parts.Add(Uri.UnescapeDataString(remainder));
                return new WebPath(parts, false);
            }

            return null;
        }

        private async ValueTask<RequestQuery?> TryReadQuery(RequestBuffer buffer)
        {
            var queryString = await ReadToken(buffer, ' ');

            if (queryString?.Length > 0)
            {
                var query = new RequestQuery();

                foreach (Match m in Pattern.GET_PARAMETER.Matches(queryString))
                {
                    query[Uri.UnescapeDataString(m.Groups[1].Value)] = Uri.UnescapeDataString(m.Groups[2].Value);
                }

                if (query.Count == 0)
                {
                    query[Uri.UnescapeDataString(queryString)] = string.Empty;
                }

                return query;
            }

            return null;
        }

        private async ValueTask<bool> TryReadHeader(RequestBuffer buffer, RequestBuilder request)
        {
            string? key, value;

            if ((key = await ReadToken(buffer, ':', 1).ConfigureAwait(false)) != null)
            {
                if ((value = await ReadToken(buffer, '\r', 1).ConfigureAwait(false)) != null)
                {
                    request.Header(key, value);
                    return true;
                }
            }

            return false;
        }

        private async ValueTask<SequencePosition?> FindPosition(RequestBuffer buffer, char delimiter)
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

            return position;
        }

        private ValueTask<string?> ReadToken(RequestBuffer buffer, char delimiter, ushort skipNext = 0, ushort skipFirst = 0, bool skipDelimiter = true)
        {
            return ReadToken(buffer, delimiter, LINE_ENDING, skipNext, skipFirst, skipDelimiter);
        }

        private async ValueTask<string?> ReadToken(RequestBuffer buffer, char delimiter, char[] boundaries, ushort skipNext = 0, ushort skipFirst = 0, bool skipDelimiter = true)
        {
            var position = await FindPosition(buffer, delimiter);

            if (position != null)
            {
                if (skipFirst > 0)
                {
                    buffer.Advance(skipFirst);
                    position = buffer.Data.PositionOf((byte)delimiter)!;
                }

                foreach (var boundary in boundaries)
                {
                    if (boundary != delimiter)
                    {
                        var boundaryPosition = buffer.Data.PositionOf((byte)boundary);

                        if (boundaryPosition != null)
                        {
                            if (position.Value.GetInteger() > boundaryPosition.Value.GetInteger())
                            {
                                return null;
                            }
                        }
                    }
                }

                var data = GetString(buffer.Data.Slice(0, position.Value));

                if (skipDelimiter)
                {
                    buffer.Advance(buffer.Data.GetPosition(1, position.Value));
                }

                if (skipNext > 0)
                {
                    buffer.Advance(skipNext);
                }

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
                    ASCII.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }

        #endregion

    }

}
