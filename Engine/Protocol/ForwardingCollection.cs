using System;
using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol
{

    /// <summary>
    /// Parses and stores forwarding information passed by proxy
    /// servers along with the request.
    /// </summary>
    internal sealed class ForwardingCollection : List<Forwarding>, IForwardingCollection
    {
        private const int DEFAULT_SIZE = 1;

        private const string HEADER_FOR = "X-Forwarded-For";
        private const string HEADER_HOST = "X-Forwarded-Host";
        private const string HEADER_PROTO = "X-Forwarded-Proto";

        internal ForwardingCollection() : base(DEFAULT_SIZE)
        {

        }

        internal void Add(string header) => AddRange(Parse(header));

        internal void TryAddLegacy(RequestHeaderCollection headers)
        {
            IPAddress? address = null;
            ClientProtocol? protocol = null;

            headers.TryGetValue(HEADER_HOST, out var host);

            if (headers.TryGetValue(HEADER_FOR, out var stringAddress))
            {
                address = ParseAddress(stringAddress);
            }

            if (headers.TryGetValue(HEADER_PROTO, out var stringProtocol))
            {
                protocol = ParseProtocol(stringProtocol);
            }

            if ((address is not null) || (host is not null) || (protocol is not null))
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

                if ((address is not null) || (host is not null) || (protocol is not null))
                {
                    yield return new Forwarding(address, host, protocol);
                }
            }
        }

        private static ClientProtocol? ParseProtocol(string? protocol)
        {
            if (protocol != null)
            {
                return string.Equals(protocol, "https", StringComparison.OrdinalIgnoreCase) ? ClientProtocol.HTTPS : ClientProtocol.HTTP;
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

}
