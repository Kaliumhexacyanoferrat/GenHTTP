using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.General;
using System;
using System.IO;

namespace GenHTTP.Core.Protocol
{

    internal class ResponseBuilder : IResponseBuilder
    {
        private FlexibleResponseStatus? _Status;

        private IResponseContent? _Content;
        private ulong? _ContentLength;
        private FlexibleContentType? _ContentType;
        private string? _ContentEncoding;

        private DateTime? _Expires;
        private DateTime? _Modified;

        private CookieCollection? _Cookies;

        private readonly HeaderCollection _Headers = new HeaderCollection();

        #region Get-/Setters

        private CookieCollection Cookies
        {
            get { return _Cookies ?? (_Cookies = new CookieCollection()); }
        }

        public IRequest Request { get; }

        #endregion

        #region Initialization

        public ResponseBuilder(IRequest request)
        {
            Request = request;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Length(ulong length)
        {
            _ContentLength = length;
            return this;
        }

        public IResponseBuilder Content(Stream body, ContentType contentType) => Content(body, new FlexibleContentType(contentType));

        public IResponseBuilder Content(Stream body, string contentType) => Content(body, new FlexibleContentType(contentType));

        public IResponseBuilder Content(Stream body, FlexibleContentType contentType)
        {
            Content(new StreamContent(body));
            return Type(contentType);
        }

        public IResponseBuilder Content(IResponseContent content)
        {
            _Content = content;
            _ContentLength = content.Length;

            return this;
        }

        public IResponseBuilder Type(FlexibleContentType contentType)
        {
            _ContentType = contentType;
            return this;
        }

        public IResponseBuilder Cookie(Cookie cookie)
        {
            Cookies[cookie.Name] = cookie;
            return this;
        }

        public IResponseBuilder Cookie(string cookie)
        {
            Cookies.Add(cookie);
            return this;
        }

        public IResponseBuilder Header(string key, string value)
        {
            _Headers[key] = value;
            return this;
        }

        public IResponseBuilder Encoding(string encoding)
        {
            _ContentEncoding = encoding;
            return this;
        }

        public IResponseBuilder Expires(DateTime expiryDate)
        {
            _Expires = expiryDate;
            return this;
        }

        public IResponseBuilder Modified(DateTime modificationDate)
        {
            _Modified = modificationDate;
            return this;
        }

        public IResponseBuilder Status(ResponseStatus status)
        {
            _Status = new FlexibleResponseStatus(status);
            return this;
        }

        public IResponseBuilder Status(int status, string reason)
        {
            _Status = new FlexibleResponseStatus(status, reason);
            return this;
        }

        public IResponse Build()
        {
            try
            {
                if (_Status == null)
                {
                    throw new BuilderMissingPropertyException("Status");
                }

                return new Response(_Status!.Value, _Headers, _Cookies)
                {
                    Content = _Content,
                    ContentEncoding = _ContentEncoding,
                    ContentLength = _ContentLength,
                    ContentType = _ContentType,
                    Expires = _Expires,
                    Modified = _Modified
                };
            }
            catch (Exception)
            {
                _Headers.Dispose();
                _Cookies?.Dispose();

                throw;
            }
        }

        #endregion

    }

}
