using System.Net;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol;

/// <summary>
/// Parses and stores forwarding information passed by proxy
/// servers along with the request.
/// </summary>
internal sealed class ForwardingCollection : List<Forwarding>, IForwardingCollection
{
    private const int DefaultSize = 1;

    private const string HeaderFor = "X-Forwarded-For";
    private const string HeaderHost = "X-Forwarded-Host";
    private const string HeaderProto = "X-Forwarded-Proto";

    internal ForwardingCollection() : base(DefaultSize)
    {

    }

    internal void Add(string header) => AddRange(Parse(header));

    internal void TryAddLegacy(RequestHeaderCollection headers)
    {
        IPAddress? address = null;
        ClientProtocol? protocol = null;

        headers.TryGetValue(HeaderHost, out var host);

        if (headers.TryGetValue(HeaderFor, out var stringAddress))
        {
            address = ParseAddress(stringAddress);
        }

        if (headers.TryGetValue(HeaderProto, out var stringProtocol))
        {
            protocol = ParseProtocol(stringProtocol);
        }

        if (address is not null || host is not null || protocol is not null)
        {
            Add(new Forwarding(address, host, protocol));
        }
    }

    private static IEnumerable<Forwarding> Parse(string value)
    {
        var forwardings = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var forwarding in forwardings)
        {
            IPAddress? address = null;
            ClientProtocol? protocol = null;

            string? host = null;

            var fields = forwarding.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var field in fields)
            {
                var kv = field.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (kv.Length == 2)
                {
                    var key = kv[0].Trim().ToLower();
                    var val = kv[1].Trim();

                    if (key == "for")
                    {
                        address = ParseAddress(val);
                    }
                    else if (key == "host")
                    {
                        host = val;
                    }
                    else if (key == "proto")
                    {
                        protocol = ParseProtocol(val);
                    }
                }
            }

            if (address is not null || host is not null || protocol is not null)
            {
                yield return new Forwarding(address, host, protocol);
            }
        }
    }

    private static ClientProtocol? ParseProtocol(string? protocol)
    {
        if (protocol != null)
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

    private static IPAddress? ParseAddress(string? address)
    {
        if (address != null)
        {
            if (IPAddress.TryParse(address, out var ip))
            {
                return ip;
            }
        }

        return null;
    }
}
