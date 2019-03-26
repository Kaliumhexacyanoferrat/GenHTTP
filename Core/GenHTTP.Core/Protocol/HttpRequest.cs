using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Core
{

    /// <summary>
    /// Provides methods to access a recieved http request.
    /// </summary>
    public class HttpRequest : IHttpRequest
    {
        
        #region Get-/Setters

        public IRoutingContext? Routing { get; set; }

        /// <summary>
        /// The protocol supported by the client.
        /// </summary>
        public ProtocolType ProtocolType { get; internal set; }
        
        /// <summary>
        /// The type of the http request.
        /// </summary>
        public RequestType Type { get; internal set; }

        /// <summary>
        /// The requested file.
        /// </summary>
        public string Path { get; internal set; }
        
        /// <summary>
        /// All available cookies.
        /// </summary>
        public CookieCollection Cookies { get; }

        /// <summary>
        /// Data submitted via POST.
        /// </summary>
        public Dictionary<string, string> PostFields { get; }

        /// <summary>
        /// Data submitted via GET.
        /// </summary>
        public Dictionary<string, string> Query { get; }
        
        /// <summary>
        /// The client handler assigned to this request.
        /// </summary>
        public IClientHandler Handler { get; internal set; }

        /// <summary>
        /// The requested host.
        /// </summary>
        /// <remarks>
        /// Used for virtual hosting.
        /// </remarks>
        public string? Host => this["Host"];

        /// <summary>
        /// The address (URI) of the resource from which the Request-URI was obtained.
        /// </summary>
        public string? Referer => this["Referer"];

        /// <summary>
        /// The user agent of the client.
        /// </summary>
        public string? UserAgent => this["User-Agent"];
        
        /// <summary>
        /// Retrieve a header field of the request.
        /// </summary>
        /// <param name="additionalHeader">The name of the header field</param>
        /// <returns>The value of the requested header field</returns>
        public string? this[string additionalHeader]
        {
            get
            {
                if (Additional.ContainsKey(additionalHeader.ToLower()))
                {
                    return Additional[additionalHeader.ToLower()];
                }

                return null;
            }
        }

        internal Dictionary<string, string> Additional = new Dictionary<string, string>();

        #endregion

        #region Initialization

        internal HttpRequest(ClientHandler handler)
        {
            Handler = handler;

            Cookies = new CookieCollection();
            PostFields = new Dictionary<string, string>();
            Query = new Dictionary<string, string>();

            Path = string.Empty;        
        }

        #endregion
                                
    }

}
