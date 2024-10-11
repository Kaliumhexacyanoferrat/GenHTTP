using System;

namespace GenHTTP.Api.Protocol;

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
    /// The headers of the HTTP response.
    /// </summary>
    IEditableHeaderCollection Headers { get; }

    /// <summary>
    /// The cookies to be sent to the client.
    /// </summary>
    ICookieCollection Cookies { get; }

    /// <summary>
    /// True, if there are cookies to be sent with this respone.
    /// </summary>
    bool HasCookies { get; }

    /// <summary>
    /// Adds the given cookie to the cookie collection of this response.
    /// </summary>
    /// <param name="cookie">The cookie to be added</param>
    void SetCookie(Cookie cookie);

    #endregion

    #region Content

    /// <summary>
    /// The type of the content.
    /// </summary>
    FlexibleContentType? ContentType { get; set; }

    /// <summary>
    /// The encoding of the content (e.g. "br").
    /// </summary>
    string? ContentEncoding { get; set; }

    /// <summary>
    /// The number of bytes the content consists of.
    /// </summary>
    ulong? ContentLength { get; set; }

    /// <summary>
    /// The response that will be sent to the requesting client.
    /// </summary>
    IResponseContent? Content { get; set; }

    #endregion

}
