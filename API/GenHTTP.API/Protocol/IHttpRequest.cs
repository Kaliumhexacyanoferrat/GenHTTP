using GenHTTP.Api.Content;
using GenHTTP.Api.Routing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
        /// Data submitted via GET.
        /// </summary>
        Dictionary<string, string> GetFields { get; }

        /// <summary>
        /// Retrieve a parameter from the request (GET or POST).
        /// </summary>
        /// <param name="name">The name of the parameter to retrive</param>
        /// <returns>The value of the requested parameter or null, if it could not be found</returns>
        /// <remarks>
        /// This method will prioritize POST parameters over GET parameters.
        /// </remarks>
        string? GetParameter(string name);

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

        IRoutingContext Routing { get; }
                
    }

}
