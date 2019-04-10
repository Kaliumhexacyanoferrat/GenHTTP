using System;
using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Protocol;
using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    internal class Response : IResponse
    {
        private CookieCollection _Cookies;
        private HeaderCollection _Headers;

        #region Get-/Setters

        public FlexibleResponseStatus Status { get; set; }

        public DateTime? Expires { get; set; }

        public DateTime? Modified { get; set; }

        public FlexibleContentType? ContentType { get; set; }

        public string? ContentEncoding { get; set; }

        public ulong? ContentLength { get; set; }

        public Stream? Content { get; set; }

        public ICookieCollection Cookies => _Cookies;

        public List<string> RawCookies { get; set; }

        public IHeaderCollection Headers => _Headers;
        
        public string? this[string field]
        {
            get
            {
                return (_Headers.ContainsKey(field)) ? _Headers[field] : null;
            }
            set
            {
                if (value != null)
                {
                    _Headers[field] = value;
                }
                else if (_Headers.ContainsKey(field))
                {
                    _Headers.Remove(field);
                }
            }
        }

        #endregion

        #region Initialization

        internal Response(FlexibleResponseStatus type, HeaderCollection headers, CookieCollection cookies, List<string> rawCookies)
        {
            Status = type;

            _Headers = headers;

            _Cookies = cookies;
            RawCookies = rawCookies;
        }

        #endregion

        #region Functionality

        public void AddCookie(Cookie cookie)
        {
            _Cookies[cookie.Name] = cookie;
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Content != null)
                    {
                        Content.Dispose();
                    }
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

}
