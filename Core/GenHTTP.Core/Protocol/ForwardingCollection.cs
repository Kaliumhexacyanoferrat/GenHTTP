using System;
using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    internal class ForwardingCollection : List<Forwarding>, IForwardingCollection
    {
        private const int DEFAULT_SIZE = 1;

        internal ForwardingCollection() : base(DEFAULT_SIZE)
        {

        }

        internal void Add(string header) => AddRange(Parse(header));
        
        private IEnumerable<Forwarding> Parse(string value)
        {
            var forwardings = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach(var forwarding in forwardings)
            {
                IPAddress? address = null;
                ClientProtocol? protocol = null;

                string? host = null;

                var fields = value.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    var kv = field.Split('=', StringSplitOptions.RemoveEmptyEntries);

                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim().ToLower();
                        var val = kv[1].Trim();

                        if (key == "for")
                        {
                            IPAddress.TryParse(val, out address);
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

                if ((address != null) || (host != null) || (protocol != null))
                {
                    yield return new Forwarding(address, host, protocol);
                }
            } 
        }

    }

}
