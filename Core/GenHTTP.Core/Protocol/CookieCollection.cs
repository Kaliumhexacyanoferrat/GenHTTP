using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;

namespace GenHTTP.Core.Protocol
{

    internal class CookieCollection : Dictionary<string, Cookie>, ICookieCollection
    {
        private const int DEFAULT_SIZE = 16;

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

        private IEnumerable<Cookie> Parse(string value)
        {
            var cookies = value.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var kv in cookies)
            {
                var index = kv.IndexOf("=");

                if (index > -1)
                {
                    var cookie = new Cookie(kv.Substring(0, index), kv.Substring(index + 1));
                    yield return cookie;
                }
            }
        }

    }

}
