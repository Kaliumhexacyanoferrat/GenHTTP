using System;
using System.Collections.Generic;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// A set of custom modifications to be applied to a response.
/// </summary>
public sealed class ResponseModifications
{

    #region Get-/Setters

    /// <summary>
    /// The status to be set on the response, if set.
    /// </summary>
    public FlexibleResponseStatus? Status { get; }

    /// <summary>
    /// The content type to be set on the response, if set.
    /// </summary>
    public FlexibleContentType? ContentType { get; }

    /// <summary>
    /// The cookies to be set on the response, if set.
    /// </summary>
    public List<Cookie>? Cookies { get; }

    /// <summary>
    /// The encoding to be set on the response, if set.
    /// </summary>
    public string? Encoding { get; }

    /// <summary>
    /// The expiration date to be set on the response, if set.
    /// </summary>
    public DateTime? ExpiryDate { get; }

    /// <summary>
    /// The modification date to be set on the response, if set.
    /// </summary>
    public DateTime? ModificationDate { get; }

    /// <summary>
    /// The headers to be set on the response, if set.
    /// </summary>
    public Dictionary<string, string>? Headers { get; }

    #endregion

    #region Initialization

    public ResponseModifications(FlexibleResponseStatus? status, FlexibleContentType? contentType,
        List<Cookie>? cookies, string? encoding, DateTime? expiryDate, DateTime? modificationDate,
        Dictionary<string, string>? headers)
    {
            Status = status;

            ContentType = contentType;
            Encoding = encoding;

            ExpiryDate = expiryDate;
            ModificationDate = modificationDate;

            Cookies = cookies;
            Headers = headers;
        }

    #endregion

    #region Functionality

    /// <summary>
    /// Applies the modifications configured in this instance to the
    /// given response.
    /// </summary>
    /// <param name="builder">The response to be adjusted</param>
    public void Apply(IResponseBuilder builder)
    {
            if (Status != null)
            {
                builder.Status(Status.Value.RawStatus, Status.Value.Phrase);
            }

            if (null != ContentType)
            {
                builder.Type(ContentType);
            }

            if (Cookies?.Count > 0)
            {
                foreach (var cookie in Cookies)
                {
                    builder.Cookie(cookie);
                }
            }

            if (Encoding != null)
            {
                builder.Encoding(Encoding);
            }

            if (ExpiryDate != null)
            {
                builder.Expires(ExpiryDate.Value);
            }

            if (ModificationDate != null)
            {
                builder.Modified(ModificationDate.Value);
            }

            if (Headers?.Count > 0)
            {
                foreach (var header in Headers)
                {
                    builder.Header(header.Key, header.Value);
                }
            }
        }

    #endregion

}
