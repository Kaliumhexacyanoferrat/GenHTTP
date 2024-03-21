using System;
using System.Collections.Generic;

namespace GenHTTP.Api.Protocol
{

    public sealed class ResponseModifications
    {

        #region Get-/Setters

        public FlexibleResponseStatus? Status { get; }

        public FlexibleContentType? ContentType { get; }

        public List<Cookie>? Cookies { get; }

        public string? Encoding { get; }

        public DateTime? ExpiryDate { get; }

        public DateTime? ModificationDate { get; }

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

}
