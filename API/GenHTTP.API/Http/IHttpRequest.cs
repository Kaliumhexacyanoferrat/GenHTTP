using GenHTTP.Api.Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace GenHTTP.Api.Http
{

    public interface IHttpRequest
    {

        /// <summary>
        /// The protocol supported by the client.
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// Check, whether this request has been redirected
        /// by a server rule.
        /// </summary>
        bool Redirected { get; }

        /// <summary>
        /// This property tells you the original requested file,
        /// if the request has been redirected by the server.
        /// The request could have been redirected multiple times,
        /// so the return type of this property is a list with all
        /// previous URLs.
        /// </summary>
        Collection<string> RedirectedFrom { get; }

        /// <summary>
        /// Specifies, whether the client is able to read compressed data (GZip).
        /// </summary>
        bool CompressionAvailable { get; }

        /// <summary>
        /// The type of the http request.
        /// </summary>
        RequestType Type { get; }

        /// <summary>
        /// The requested file.
        /// </summary>
        string File { get; }

        /// <summary>
        /// Retrieve a header field of the request.
        /// </summary>
        /// <param name="additionalHeader">The name of the header field</param>
        /// <returns>The value of the requested header field</returns>
        string this[string additionalHeader] { get; }

        /// <summary>
        /// The user agent of the client.
        /// </summary>
        string UserAgent { get; }

        /// <summary>
        /// All available cookies.
        /// </summary>
        CookieCollection Cookies { get; }

        /// <summary>
        /// The project this request relates to.
        /// </summary>
        IProject Project { get; }

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
        string GetParameter(string name);

        /// <summary>
        /// The address (URI) of the resource from which the Request-URI was obtained.
        /// </summary>
        string Referer { get; }

        /// <summary>
        /// The requested host.
        /// </summary>
        /// <remarks>
        /// Used for virtual hosting.
        /// </remarks>
        string Host { get; }

        /// <summary>
        /// Whether this request was redirected due to virtual hosting.
        /// </summary>
        bool VirtualHosting { get; }

        /// <summary>
        /// Redirect this request.
        /// </summary>
        /// <param name="toURL">The new URL</param>
        void Redirect(string toURL);

        /// <summary>
        /// Allows you to normalize an url, if virtual hosting is used.
        /// </summary>
        /// <param name="url">The url to normalize</param>
        /// <returns>The normalized url</returns>
        /// <remarks>
        /// If the requested file lies on a virtual host, it does not need
        /// the name of the project in the url. You should use this method
        /// to calculate all of your urls.
        /// </remarks>
        string NormalizeAbsoluteUrl(string url);

    }

}
