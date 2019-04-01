using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Available response types.
    /// </summary>
    public enum ResponseType : int
    {

        OK = 200,

        Created = 201,

        Accepted = 202,

        NoContent = 204,

        MovedPermanently = 301,

        MovedTemporarily = 302,

        NotModified = 304,

        BadRequest = 400,

        Unauthorized = 401,

        Forbidden = 403,

        NotFound = 404,

        MethodNotAllowed = 405,

        InternalServerError = 500,

        NotImplemented = 501,

        BadGateway = 502,

        ServiceUnavailable = 503

    }

}
