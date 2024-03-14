using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Protocol;

namespace GenHTTP.Engine
{

    internal sealed class Response : IResponse
    {
        private static readonly FlexibleResponseStatus STATUS_OK = new(ResponseStatus.OK);

        private CookieCollection? _Cookies;

        private readonly ResponseHeaderCollection _Headers = new();

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
                if (_Headers.TryGetValue(field, out var value))
                {
                    return value;
                }

                return null;
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

        #region Functionality

        public void SetCookie(Cookie cookie)
        {
            WriteableCookies[cookie.Name] = cookie;
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                Headers.Dispose();

                _Cookies?.Dispose();

                if (Content is IDisposable disposableContent)
                {
                    disposableContent.Dispose();
                }

                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
