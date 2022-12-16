using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Engine.Infrastructure.Configuration;

namespace GenHTTP.Engine.Protocol
{

    /// <summary>
    /// Reads the next HTTP request to be handled by the server from
    /// the client connection. 
    /// </summary>
    /// <remarks>
    /// Be aware that this code path is heavily optimized for low
    /// memory allocations. Changes to this class should allocate
    /// as few memory as possible to avoid the performance of
    /// the server from being impacted in a negative manner.
    /// </remarks>
    internal sealed class RequestParser
    {
        private static readonly char[] LINE_ENDING = new char[] { '\r' };
        private static readonly char[] PATH_ENDING = new char[] { '\r', '\n', '?', ' ' };

        private static readonly Encoding ASCII = Encoding.ASCII;

        private RequestBuilder? _Builder;

        #region Get-/Setters

        private NetworkConfiguration Configuration { get; }

        private RequestBuilder Request => _Builder ??= new();

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

            if ((method = await ReadToken(buffer, ' ').ConfigureAwait(false)) is not null)
            {
                Request.Type(method);
            }
            else
            {
                return null;
            }

            if ((path = await TryReadPath(buffer)) is not null)
            {
                Request.Path(path);
            }
            else
            {
                return null;
            }

            if ((query = await TryReadQuery(buffer)) is not null)
            {
                Request.Query(query);
            }

            if ((protocol = await ReadToken(buffer, '\r', 1, 5)) is not null)
            {
                Request.Protocol(protocol);
            }
            else
            {
                return null;
            }

            while (await TryReadHeader(buffer, Request)) { /* nop */ }

            if ((await ReadToken(buffer, '\r', 1)) is null)
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

                        Request.Content(await parser.GetBody(buffer));
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

        private static async ValueTask<WebPath?> TryReadPath(RequestBuffer buffer)
        {
            // ignore the slash at the beginning
            buffer.Advance(1);

            var parts = new List<WebPathPart>(4);

            string? part;

            while ((part = await ReadToken(buffer, '/', PATH_ENDING).ConfigureAwait(false)) is not null)
            {
                parts.Add(new WebPathPart(part));
            }

            // find the delimiter (either '?' or ' ')
            var remainder = await ReadToken(buffer, '?') ?? await ReadToken(buffer, ' ');

            if (remainder is not null)
            {
                if (string.IsNullOrEmpty(remainder))
                {
                    return new WebPath(parts, true);
                }

                parts.Add(new WebPathPart(remainder));
                return new WebPath(parts, false);
            }

            return null;
        }

        private static async ValueTask<RequestQuery?> TryReadQuery(RequestBuffer buffer)
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

        private static async ValueTask<bool> TryReadHeader(RequestBuffer buffer, RequestBuilder request)
        {
            string? key, value;

            if ((key = await ReadToken(buffer, ':', 1).ConfigureAwait(false)) is not null)
            {
                if ((value = await ReadToken(buffer, '\r', 1)) is not null)
                {
                    request.Header(key, value);
                    return true;
                }
            }

            return false;
        }

        private static async ValueTask<SequencePosition?> FindPosition(RequestBuffer buffer, char delimiter)
        {
            if (buffer.ReadRequired)
            {
                if ((await buffer.Read().ConfigureAwait(false)) is null)
                {
                    return null;
                }
            }

            var position = buffer.Data.PositionOf((byte)delimiter);

            if (position is null)
            {
                if ((await buffer.Read(true).ConfigureAwait(false)) is null)
                {
                    return null;
                }

                position = buffer.Data.PositionOf((byte)delimiter);
            }

            return position;
        }

        private static ValueTask<string?> ReadToken(RequestBuffer buffer, char delimiter, ushort skipNext = 0, ushort skipFirst = 0, bool skipDelimiter = true)
        {
            return ReadToken(buffer, delimiter, LINE_ENDING, skipNext, skipFirst, skipDelimiter);
        }

        private static async ValueTask<string?> ReadToken(RequestBuffer buffer, char delimiter, char[] boundaries, ushort skipNext = 0, ushort skipFirst = 0, bool skipDelimiter = true)
        {
            var position = await FindPosition(buffer, delimiter).ConfigureAwait(false);

            if (position is not null)
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

                        if (boundaryPosition is not null)
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

        private static string GetString(ReadOnlySequence<byte> buffer)
        {
            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    ASCII.GetChars(segment.Span, span);
                    span = span[segment.Length..];
                }
            });
        }

        #endregion

    }

}
