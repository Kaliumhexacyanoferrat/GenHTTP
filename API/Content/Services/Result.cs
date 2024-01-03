using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Services
{

    public class Result<T> : IResult, IResponseModification<Result<T>>
    {
        private FlexibleResponseStatus? _Status;

        private Dictionary<string, string>? _Headers;

        private DateTime? _Expires;

        private DateTime? _Modified;

        private List<Cookie>? _Cookies;

        private FlexibleContentType? _ContentType;

        private string? _Encoding;

        #region Get-/Setters

        public T? Payload { get; }

        object? IResult.Payload => Payload;

        #endregion

        #region Initialization

        public Result(T? payload)
        {
            Payload = payload;
        }

        #endregion

        #region Functionality

        public Result<T> Status(ResponseStatus status)
        {
            _Status = new(status);
            return this;
        }

        public Result<T> Status(int status, string reason)
        {
            _Status = new FlexibleResponseStatus(status, reason);
            return this;
        }

        public Result<T> Header(string key, string value)
        {
            if (_Headers == null)
            {
                _Headers = new();
            }

            _Headers[key] = value;

            return this;
        }

        public Result<T> Expires(DateTime expiryDate)
        {
            _Expires = expiryDate;
            return this;
        }

        public Result<T> Modified(DateTime modificationDate)
        {
            _Modified = modificationDate;
            return this;
        }

        public Result<T> Cookie(Cookie cookie)
        {
            if (_Cookies == null)
            {
                _Cookies = new();
            }

            _Cookies.Add(cookie);

            return this;
        }

        public Result<T> Type(FlexibleContentType contentType)
        {
            _ContentType = contentType;
            return this;
        }

        public Result<T> Encoding(string encoding)
        {
            _Encoding = encoding;
            return this;
        }

        public void Apply(IResponseBuilder builder)
        {
            if (_Status != null)
            {
                var value = _Status.Value;

                builder.Status(value.RawStatus, value.Phrase);
            }

            if (_Headers != null)
            {
                foreach (var kv in _Headers)
                {
                    builder.Header(kv.Key, kv.Value);
                }
            }

            if (_Expires != null)
            {
                builder.Expires(_Expires.Value);
            }

            if (_Modified != null)
            {
                builder.Modified(_Modified.Value);
            }

            if (_Cookies != null)
            {
                foreach (var cookie in _Cookies)
                {
                    builder.Cookie(cookie);
                }
            }

            if (_ContentType is not null)
            {
                builder.Type(_ContentType);
            }

            if (_Encoding != null)
            {
                builder.Encoding(_Encoding);
            }
        }

        #endregion

    }

}
