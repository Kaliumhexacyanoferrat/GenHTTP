using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;
using GenHTTP.Core.Utilities;

namespace GenHTTP.Core.Protocol
{

    internal class CookieCollection : PooledDictionary<string, Cookie>, ICookieCollection
    {
        internal const int DEFAULT_SIZE = 4;

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

        private List<Cookie> Parse(string value)
        {
            var result = new List<Cookie>(2);

            var cookies = value.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var kv in cookies)
            {
                var index = kv.IndexOf("=");

                if (index > -1)
                {
                    result.Add(new Cookie(kv.Substring(0, index), kv.Substring(index + 1)));
                }
            }

            return result;
        }

    }

}
