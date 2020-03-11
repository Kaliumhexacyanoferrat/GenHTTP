using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Protocol;
using GenHTTP.Core.Utilities;

namespace GenHTTP.Core.Protocol
{

    internal class NoCookieCollection : ICookieCollection, IDisposable
    {

        #region Get-/Setters

        public Cookie this[string key] => throw new KeyNotFoundException();

        public IEnumerable<string> Keys => Enumerable.Empty<string>();

        public IEnumerable<Cookie> Values => Enumerable.Empty<Cookie>();

        public int Count => 0;

        #endregion

        #region Functionality

        public bool ContainsKey(string key)
        {
            return false;
        }

        public bool TryGetValue(string key, out Cookie value)
        {
            value = new Cookie("no", "cookie");
            return false;
        }

        public IEnumerator<KeyValuePair<string, Cookie>> GetEnumerator() => EmptyEnumerator<KeyValuePair<string, Cookie>>.Instance;

        IEnumerator IEnumerable.GetEnumerator() => EmptyEnumerator.Instance;

        public void Dispose()
        {
            // nothing to do
        }

        #endregion

    }

}
