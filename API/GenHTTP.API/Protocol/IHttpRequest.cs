using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Protocol
{

    public interface IHttpRequest
    {

        /// <summary>
        /// The protocol supported by the client.
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// The type of the http request.
        /// </summary>
        RequestType Type { get; }

        /// <summary>
        /// The requested path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Data submitted via GET.
        /// </summary>
        Dictionary<string, string> Query { get; }

        /// <summary>
        /// Retrieve a header field of the request.
        /// </summary>
        /// <param name="additionalHeader">The name of the header field</param>
        /// <returns>The value of the requested header field</returns>
        string? this[string additionalHeader] { get; }

        /// <summary>
        /// The user agent of the client.
        /// </summary>
        string? UserAgent { get; }

        /// <summary>
        /// All available cookies.
        /// </summary>
        CookieCollection Cookies { get; }

        /// <summary>
        /// Data submitted via POST.
        /// </summary>
        Dictionary<string, string> PostFields { get; }
                
        /// <summary>
        /// The address (URI) of the resource from which the Request-URI was obtained.
        /// </summary>
        string? Referer { get; }

        /// <summary>
        /// The requested host.
        /// </summary>
        string? Host { get; }

        /// <summary>
        /// The client handler assigned to this request.
        /// </summary>
        IClientHandler Handler { get; }

        IRoutingContext? Routing { get; }
                
    }

}
