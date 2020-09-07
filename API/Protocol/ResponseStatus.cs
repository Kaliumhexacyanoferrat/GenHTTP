using System;
using System.Collections.Generic;

namespace GenHTTP.Api.Protocol
{

    #region Known Types

    public enum ResponseStatus
    {

        Continue = 100,

        SwitchingProtocols = 101,

        Processing = 102,

        OK = 200,

        Created = 201,

        Accepted = 202,

        NoContent = 204,

        PartialContent = 206,

        MultiStatus = 207,

        AlreadyReported = 208,

        MovedPermanently = 301,

        Found = 302,

        SeeOther = 303,

        NotModified = 304,

        TemporaryRedirect = 307,

        PermanentRedirect = 308,

        BadRequest = 400,

        Unauthorized = 401,

        Forbidden = 403,

        NotFound = 404,

        MethodNotAllowed = 405,

        NotAcceptable = 406,

        ProxyAuthenticationRequired = 407,

        Conflict = 409,

        Gone = 410,

        LengthRequired = 411,

        PreconditionFailed = 412,

        RequestEntityTooLarge = 413,

        RequestUriTooLong = 414,

        UnsupportedMediaType = 415,

        RequestedRangeNotSatisfiable = 416,

        ExpectationFailed = 417,

        UnprocessableEntity = 422,

        Locked = 423,

        FailedDependency = 424,

        ReservedForWebDAV = 425,

        UpgradeRequired = 426,

        PreconditionRequired = 428,

        TooManyRequests = 429,

        RequestHeaderFieldsTooLarge = 431,

        UnavailableForLegalReasons = 451,

        InternalServerError = 500,

        NotImplemented = 501,

        BadGateway = 502,

        ServiceUnavailable = 503,

        GatewayTimeout = 504,

        HttpVersionNotSupported = 505,

        InsufficientStorage = 507,

        LoopDetected = 508,

        NotExtended = 510,

        NetworkAuthenticationRequired = 511

    }

    #endregion

    /// <summary>
    /// The status of the response send to the client.
    /// </summary>
    public struct FlexibleResponseStatus
    {

        #region Get-/Setters

        /// <summary>
        /// The known status, if any.
        /// </summary>
        public ResponseStatus? KnownStatus { get; }

        /// <summary>
        /// The raw HTTP status.
        /// </summary>
        public int RawStatus { get; }

        /// <summary>
        /// The reason phrase to be send.
        /// </summary>
        public string Phrase { get; }

        #endregion

        #region Mapping

        private static readonly Dictionary<ResponseStatus, string> MAPPING = new Dictionary<ResponseStatus, string>
        {
            { ResponseStatus.Accepted, "Accepted" },
            { ResponseStatus.BadGateway, "Bad Gateway" },
            { ResponseStatus.BadRequest, "Bad Request" },
            { ResponseStatus.Created, "Created" },
            { ResponseStatus.Forbidden, "Forbidden" },
            { ResponseStatus.InternalServerError, "Internal Server Error" },
            { ResponseStatus.MethodNotAllowed, "Method Not Allowed" },
            { ResponseStatus.MovedPermanently, "Moved Permamently" },
            { ResponseStatus.Found, "Found" },
            { ResponseStatus.NoContent, "No Content" },
            { ResponseStatus.NotFound, "Not Found" },
            { ResponseStatus.NotImplemented, "Not Implemented" },
            { ResponseStatus.NotModified, "Not Modified" },
            { ResponseStatus.OK, "OK" },
            { ResponseStatus.ServiceUnavailable, "Service Unavailable"},
            { ResponseStatus.Unauthorized, "Unauthorized"},
            { ResponseStatus.PartialContent, "Partial Content"},
            { ResponseStatus.MultiStatus, "Multi-Status"},
            { ResponseStatus.AlreadyReported, "Already Reported"},
            { ResponseStatus.SeeOther, "See Other" },
            { ResponseStatus.TemporaryRedirect, "Temporary Redirect"},
            { ResponseStatus.PermanentRedirect, "Permanent Redirect"},
            { ResponseStatus.Continue, "Continue" },
            { ResponseStatus.SwitchingProtocols, "Switching Protocols" },
            { ResponseStatus.NotAcceptable, "Not Acceptable" },
            { ResponseStatus.ProxyAuthenticationRequired, "Proxy Authentication Required" },
            { ResponseStatus.Conflict, "Conflict" },
            { ResponseStatus.Gone, "Gone" },
            { ResponseStatus.LengthRequired, "Length Required" },
            { ResponseStatus.PreconditionFailed, "Precondition Failed" },
            { ResponseStatus.RequestEntityTooLarge, "Request Entity Too Large" },
            { ResponseStatus.RequestUriTooLong, "Request Uri Too Long" },
            { ResponseStatus.UnsupportedMediaType, "Unsupported Media Type" },
            { ResponseStatus.RequestedRangeNotSatisfiable, "Requested Range Not Satisfiable" },
            { ResponseStatus.ExpectationFailed, "Expectation Failed" },
            { ResponseStatus.UnprocessableEntity, "Unprocessable Entity" },
            { ResponseStatus.Locked, "Locked" },
            { ResponseStatus.FailedDependency, "Failed Dependency" },
            { ResponseStatus.ReservedForWebDAV, "Reserved For WebDAV" },
            { ResponseStatus.UpgradeRequired, "Upgrade Required" },
            { ResponseStatus.PreconditionRequired, "Precondition Required" },
            { ResponseStatus.TooManyRequests, "Too Many Requests" },
            { ResponseStatus.RequestHeaderFieldsTooLarge, "Request Header Fields Too Large" },
            { ResponseStatus.UnavailableForLegalReasons, "Unavailable For Legal Reasons" },
            { ResponseStatus.GatewayTimeout, "Gateway Timeout" },
            { ResponseStatus.HttpVersionNotSupported, "HTTP Version Not Supported" },
            { ResponseStatus.InsufficientStorage, "Insufficient Storage" },
            { ResponseStatus.LoopDetected, "Loop Detected" },
            { ResponseStatus.NotExtended, "Not Extended" },
            { ResponseStatus.NetworkAuthenticationRequired, "Network Authentication Required" }
        };

        #endregion

        #region Initialization

        public FlexibleResponseStatus(int status, string phrase)
        {
            RawStatus = status;
            Phrase = phrase;

            if (Enum.IsDefined(typeof(ResponseStatus), status))
            {
                KnownStatus = (ResponseStatus)status;
            }
            else
            {
                KnownStatus = null;
            }
        }

        public FlexibleResponseStatus(ResponseStatus status)
        {
            KnownStatus = status;

            RawStatus = (int)status;
            Phrase = MAPPING[status];
        }

        #endregion

        #region Convenience

        public static bool operator ==(FlexibleResponseStatus status, ResponseStatus knownStatus) => status.KnownStatus == knownStatus;

        public static bool operator !=(FlexibleResponseStatus status, ResponseStatus knownStatus) => status.KnownStatus != knownStatus;

        public static bool operator ==(FlexibleResponseStatus status, int rawStatus) => status.RawStatus == rawStatus;

        public static bool operator !=(FlexibleResponseStatus status, int rawStatus) => status.RawStatus != rawStatus;

        public override bool Equals(object obj) => obj is FlexibleResponseStatus status && RawStatus == status.RawStatus;

        public override int GetHashCode() => RawStatus.GetHashCode();

        #endregion

    }

}
