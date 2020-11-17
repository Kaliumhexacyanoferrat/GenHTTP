using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Protocol;

namespace GenHTTP.Engine
{

    internal class Response : IResponse
    {
        private static FlexibleResponseStatus STATUS_OK = new(ResponseStatus.OK);

        private CookieCollection? _Cookies;

        private readonly HeaderCollection _Headers = new();

        #region Get-/Setters

        public FlexibleResponseStatus Status { get; set; }

        public DateTime? Expires { get; set; }

        public DateTime? Modified { get; set; }

        public FlexibleContentType? ContentType { get; set; }

        public string? ContentEncoding { get; set; }

        public ulong? ContentLength { get; set; }

        public IResponseContent? Content { get; set; }

        public ICookieCollection Cookies => WriteableCookies;

        public bool HasCookies => (_Cookies is not null) && (_Cookies.Count > 0);

        public IEditableHeaderCollection Headers => _Headers;

        public string? this[string field]
        {
            get
            {
                return (_Headers.ContainsKey(field)) ? _Headers[field] : null;
            }
            set
            {
                if (value is not null)
                {
                    _Headers[field] = value;
                }
                else if (_Headers.ContainsKey(field))
                {
                    _Headers.Remove(field);
                }
            }
        }

        internal CookieCollection WriteableCookies
        {
            get { return _Cookies ??= new(); }
        }

        #endregion

        #region Initialization

        internal Response()
        {
            Status = STATUS_OK;
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
                    Headers.Dispose();

                    _Cookies?.Dispose();

                    if (Content is IDisposable disposableContent)
                    {
                        disposableContent.Dispose();
                    }
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
