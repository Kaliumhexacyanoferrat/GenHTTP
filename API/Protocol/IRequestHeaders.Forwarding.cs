using System.Net;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Allows the chain of reverse proxies that forwarded a request to be read
/// from a header list (e.g. the "Forwarded" or "X-Forwarded-*" request headers).
/// </summary>
public static class ForwardingHeaderExtensions
{

    #region Functionality

    /// <summary>
    /// Reads the proxy chain that forwarded the request from the given headers.
    /// </summary>
    /// <param name="headers">The headers to search (typically <see cref="IRequestHeader.Headers"/>)</param>
    /// <returns>
    /// One entry per hop, ordered as found in the headers. Supports both the
    /// "Forwarded" header defined by RFC 7239 and the legacy "X-Forwarded-*"
    /// headers (used if no "Forwarded" header is present).
    /// </returns>
    public static List<Forwarding> GetForwardings(this IRequestHeaders headers)
    {
        var result = new List<Forwarding>();

        var hasForwardedHeader = false;

        for (var i = 0; i < headers.Count; i++)
        {
            var entry = headers[i];

            if (entry.Key == KnownHeaders.Forwarded)
            {
                hasForwardedHeader = true;

                ParseForwarded(entry.Value.ToString(), result);
            }
        }

        if (!hasForwardedHeader)
        {
            ParseLegacy(headers, result);
        }

        return result;
    }

    #endregion

    #region Parsing

    private static void ParseForwarded(string value, List<Forwarding> target)
    {
        var elements = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var element in elements)
        {
            IPAddress? forAddress = null;
            IPAddress? byAddress = null;

            string? host = null;

            ClientProtocol? protocol = null;

            var fields = element.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var field in fields)
            {
                var separator = field.IndexOf('=');

                if (separator <= 0)
                {
                    continue;
                }

                var key = field[..separator].Trim().ToLowerInvariant();
                var val = field[(separator + 1)..].Trim().Trim('"');

                switch (key)
                {
                    case "for":
                        forAddress = ParseNode(val);
                        break;
                    case "by":
                        byAddress = ParseNode(val);
                        break;
                    case "host":
                        host = val;
                        break;
                    case "proto":
                        protocol = ParseProtocol(val);
                        break;
                }
            }

            if (forAddress is not null || byAddress is not null || host is not null || protocol is not null)
            {
                target.Add(new Forwarding(forAddress, byAddress, host, protocol));
            }
        }
    }

    private static void ParseLegacy(IKeyValueList headers, List<Forwarding> target)
    {
        var forHeader = headers.GetEntry(KnownHeaders.ForwardedFor)?.ToString();
        var hostHeader = headers.GetEntry(KnownHeaders.ForwardedHost)?.ToString();
        var protoHeader = headers.GetEntry(KnownHeaders.ForwardedProto)?.ToString();

        IPAddress? address = null;

        if (forHeader is not null)
        {
            var first = forHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();

            if (first is not null)
            {
                address = ParseNode(first);
            }
        }

        var host = hostHeader?.Trim();

        ClientProtocol? protocol = null;

        if (protoHeader is not null)
        {
            var first = protoHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();

            if (first is not null)
            {
                protocol = ParseProtocol(first);
            }
        }

        if (address is not null || host is not null || protocol is not null)
        {
            target.Add(new Forwarding(address, null, host, protocol));
        }
    }

    private static IPAddress? ParseNode(string raw)
    {
        var value = raw.Trim().Trim('"');

        if (value.Length == 0)
        {
            return null;
        }

        if (value[0] == '[')
        {
            var end = value.IndexOf(']');

            if (end > 0)
            {
                value = value[1..end];
            }
        }
        else
        {
            var colon = value.IndexOf(':');

            if (colon > 0 && value.IndexOf(':', colon + 1) < 0)
            {
                value = value[..colon];
            }
        }

        return IPAddress.TryParse(value, out var ip) ? ip : null;
    }

    private static ClientProtocol? ParseProtocol(string? protocol)
    {
        if (protocol is not null)
        {
            if (string.Equals(protocol, "https", StringComparison.OrdinalIgnoreCase))
            {
                return ClientProtocol.Https;
            }

            if (string.Equals(protocol, "http", StringComparison.OrdinalIgnoreCase))
            {
                return ClientProtocol.Http;
            }
        }

        return null;
    }

    #endregion

}
