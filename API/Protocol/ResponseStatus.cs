namespace GenHTTP.Api.Protocol;

public enum ResponseStatus
{

    Continue = 100,

    SwitchingProtocols = 101,

    Processing = 102,

    Ok = 200,

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

    ReservedForWebDav = 425,

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
