using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol
{

    internal sealed class CookieCollection : PooledDictionary<string, Cookie>, ICookieCollection
    {
        internal const int DEFAULT_SIZE = 6;

        internal CookieCollection() : base(DEFAULT_SIZE, StringComparer.InvariantCultureIgnoreCase)
        {

        }

        internal void Add(string header)
        {
            foreach (var cookie in Parse(header))
            {
                if (!ContainsKey(cookie.Name))
                {
                    Add(cookie.Name, cookie);
                }
            }
        }

        private static List<Cookie> Parse(string value)
        {
            var result = new List<Cookie>(2);

            var cookies = value.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var kv in cookies)
            {
                var index = kv.IndexOf("=");

                if (index > -1)
                {
                    result.Add(new(kv[..index], kv[(index + 1)..]));
                }
            }

            return result;
        }

    }

}
