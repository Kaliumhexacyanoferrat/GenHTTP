using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// The response to be send to the connected client for a given request.
    /// </summary>
    public interface IResponse : IDisposable
    {

        #region Protocol

        /// <summary>
        /// The HTTP response code.
        /// </summary>
        FlexibleResponseStatus Status { get; set; }

        #endregion

        #region Headers

        /// <summary>
        /// Define, when this resource will expire.
        /// </summary>
        DateTime? Expires { get; set; }

        /// <summary>
        /// Define, when this ressource has been changed the last time.
        /// </summary>
        DateTime? Modified { get; set; }

        /// <summary>
        /// Retrieve or set the value of a header field.
        /// </summary>
        /// <param name="field">The name of the header field</param>
        /// <returns>The value of the header field</returns>
        string? this[string field] { get; set; }

        /// <summary>
        /// Add a cookie to send to the client.
        /// </summary>
        /// <param name="cookie">The cookie to send</param>
        void AddCookie(Cookie cookie);

        /// <summary>
        /// The headers of the HTTP response.
        /// </summary>
        IHeaderCollection Headers { get; }

        /// <summary>
        /// The cookies to be send to the client.
        /// </summary>
        ICookieCollection Cookies { get; }

        /// <summary>
        /// Raw cookie headers.
        /// </summary>
        List<string> RawCookies { get; }

        #endregion

        #region Content

        /// <summary>
        /// The type of the content.
        /// </summary>
        FlexibleContentType? ContentType { get; set; }

        string? ContentEncoding { get; set; }

        /// <summary>
        /// The number of bytes the content consists of.
        /// </summary>
        ulong? ContentLength { get; set; }

        /// <summary>
        /// The response that will be sent to the requesting client.
        /// </summary>
        Stream? Content { get; set; }

        #endregion

    }

}
