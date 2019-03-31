using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    /// <summary>
    /// Provides methods to access a recieved http request.
    /// </summary>
    internal class Request : IRequest
    {
        
        #region Get-/Setters

        public IRoutingContext? Routing { get; set; }

        public ProtocolType ProtocolType { get; }
        
        public RequestType Type { get;  }

        public string Path { get; }
        
        public ICookieCollection Cookies { get; }

        public IHeaderCollection Headers { get; }
        
        public IReadOnlyDictionary<string, string> Query { get; }
        
        public IClientHandler Handler { get; }

        public Stream? Content { get; }

        public string? Host => this["Host"];

        public string? Referer => this["Referer"];

        public string? UserAgent => this["User-Agent"];
        
        public string? this[string additionalHeader]
        {
            get
            {
                if (Headers.ContainsKey(additionalHeader.ToLower()))
                {
                    return Headers[additionalHeader.ToLower()];
                }

                return null;
            }
        }

        #endregion

        #region Initialization

        internal Request(ProtocolType protocol, RequestType type, string path, IHeaderCollection headers,
                         ICookieCollection cookies, IReadOnlyDictionary<string, string> query, Stream? content, IClientHandler handler)
        {
            Handler = handler;

            ProtocolType = protocol;
            Type = type;
            Path = path;

            Cookies = cookies;
            Headers = headers;
            Query = query;

            Content = content;
        }

        #endregion

        #region Functionality
        
        public IResponseBuilder Respond()
        {
            return new ResponseBuilder(this).Type(ResponseType.OK);
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
