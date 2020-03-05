using System;
using System.IO;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Allows to configure a HTTP response to be send.
    /// </summary>
    public interface IResponseBuilder : IBuilder<IResponse>
    {

        /// <summary>
        /// The request the response belongs to.
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// Specifies the HTTP status code of the response.
        /// </summary>
        /// <param name="status">The HTTP status code of the response</param>
        IResponseBuilder Status(ResponseStatus status);

        /// <summary>
        /// Specifies the HTTP status code of the response.
        /// </summary>
        /// <param name="status">The status code of the response</param>
        /// <param name="reason">The reason phrase of the response (such as "Not Found" for 404)</param>
        IResponseBuilder Status(int status, string reason);

        /// <summary>
        /// Sets the given header field on the response. Changing HTTP
        /// protocol headers may cause incorrect behavior.
        /// </summary>
        /// <param name="key">The name of the header to be set</param>
        /// <param name="value">The value of the header field</param>
        IResponseBuilder Header(string key, string value);

        /// <summary>
        /// Sets the expiration date of the response.
        /// </summary>
        /// <param name="expiryDate">The expiration date of the response</param>
        IResponseBuilder Expires(DateTime expiryDate);

        /// <summary>
        /// Sets the point in time when the requested resource has been
        /// modified last.
        /// </summary>
        /// <param name="modificationDate">The point in time when the requested resource has been modified last</param>
        IResponseBuilder Modified(DateTime modificationDate);

        /// <summary>
        /// Adds the given cookie to the response.
        /// </summary>
        /// <param name="cookie">The cookie to be added</param>
        IResponseBuilder Cookie(Cookie cookie);

        /// <summary>
        /// Adds the given raw cookie to the resonse.
        /// </summary>
        /// <param name="cookie">The raw cookie string to be added</param>
        IResponseBuilder Cookie(string cookie);

        /// <summary>
        /// Specifies the content to be send to the client.
        /// </summary>
        /// <param name="body">The content to be send to the client</param>
        /// <param name="contentType">The type of content</param>
        [Obsolete("Replaced by the IResponseContent API")]
        IResponseBuilder Content(Stream body, ContentType contentType);

        /// <summary>
        /// Specifies the content to be send to the client.
        /// </summary>
        /// <param name="body">The content to be send to the client</param>
        /// <param name="contentType">The type of content</param>
        [Obsolete("Replaced by the IResponseContent API")]
        IResponseBuilder Content(Stream body, string contentType);

        // ToDo: Documentation

        IResponseBuilder Content(IResponseContent content);

        IResponseBuilder Type(FlexibleContentType contentType);

        // ToDo: Extensions!

        IResponseBuilder Type(ContentType contentType) => Type(new FlexibleContentType(contentType));

        IResponseBuilder Type(string contentType) => Type(new FlexibleContentType(contentType));
        
        /// <summary>
        /// Specifies the length of the content stream, if known.
        /// </summary>
        /// <param name="length">The length of the content stream</param>
        IResponseBuilder Length(ulong length);

        /// <summary>
        /// Sets the encoding of the content.
        /// </summary>
        /// <param name="encoding">The encoding of the content</param>
        IResponseBuilder Encoding(string encoding);

    }

}
