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

        internal ForwardingCollection() : base(DEFAULT_SIZE)
        {

        }

        internal void Add(string header) => AddRange(Parse(header));

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
                            if (!IPAddress.TryParse(val, out address))
                            {
                                address = null;
                            }
                        }
                        else if (key == "host")
                        {
                            host = val;
                        }
                        else if (key == "proto")
                        {
                            protocol = (val.ToLower() == "https") ? ClientProtocol.HTTPS : ClientProtocol.HTTP;
                        }
                    }
                }

                if ((address is not null) || (host is not null) || (protocol is not null))
                {
                    yield return new Forwarding(address, host, protocol);
                }
            }
        }

    }

}
