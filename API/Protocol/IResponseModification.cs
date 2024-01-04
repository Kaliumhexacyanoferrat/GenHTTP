using System;

namespace GenHTTP.Api.Protocol
{
    
    /// <summary>
    /// The protocol allowing to manipulate the response sent by
    /// the server.
    /// </summary>
    /// <typeparam name="T">The type of builder used as a return value</typeparam>
    public interface IResponseModification<out T>
    {

        /// <summary>
        /// Specifies the HTTP status code of the response.
        /// </summary>
        /// <param name="status">The HTTP status code of the response</param>
        T Status(ResponseStatus status);

        /// <summary>
        /// Specifies the HTTP status code of the response.
        /// </summary>
        /// <param name="status">The status code of the response</param>
        /// <param name="reason">The reason phrase of the response (such as "Not Found" for 404)</param>
        T Status(int status, string reason);

        /// <summary>
        /// Sets the given header field on the response. Changing HTTP
        /// protocol headers may cause incorrect behavior.
        /// </summary>
        /// <param name="key">The name of the header to be set</param>
        /// <param name="value">The value of the header field</param>
        T Header(string key, string value);

        /// <summary>
        /// Sets the expiration date of the response.
        /// </summary>
        /// <param name="expiryDate">The expiration date of the response</param>
        T Expires(DateTime expiryDate);

        /// <summary>
        /// Sets the point in time when the requested resource has been
        /// modified last.
        /// </summary>
        /// <param name="modificationDate">The point in time when the requested resource has been modified last</param>
        T Modified(DateTime modificationDate);

        /// <summary>
        /// Adds the given cookie to the response.
        /// </summary>
        /// <param name="cookie">The cookie to be added</param>
        T Cookie(Cookie cookie);

        /// <summary>
        /// Specifies the content type of this response.
        /// </summary>
        /// <param name="contentType">The content type of this response</param>
        T Type(FlexibleContentType contentType);

        /// <summary>
        /// Sets the encoding of the content.
        /// </summary>
        /// <param name="encoding">The encoding of the content</param>
        T Encoding(string encoding);

    }

}
