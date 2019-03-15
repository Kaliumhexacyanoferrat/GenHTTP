using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Http
{

    /// <summary>
    /// Available response types.
    /// </summary>
    [Serializable]
    public enum ResponseType
    {
        /// <summary>
        /// 200
        /// </summary>
        OK,
        /// <summary>
        /// 201
        /// </summary>
        Created,
        /// <summary>
        /// 202
        /// </summary>
        Accepted,
        /// <summary>
        /// 204
        /// </summary>
        NoContent,
        /// <summary>
        /// 301
        /// </summary>
        MovedPermanently,
        /// <summary>
        /// 302
        /// </summary>
        MovedTemporarily,
        /// <summary>
        /// 304
        /// </summary>
        NotModified,
        /// <summary>
        /// 400
        /// </summary>
        BadRequest,
        /// <summary>
        /// 401
        /// </summary>
        Unauthorized,
        /// <summary>
        /// 403
        /// </summary>
        Forbidden,
        /// <summary>
        /// 404
        /// </summary>
        NotFound,
        /// <summary>
        /// 500
        /// </summary>
        InternalServerError,
        /// <summary>
        /// 501
        /// </summary>
        NotImplemented,
        /// <summary>
        /// 502
        /// </summary>
        BadGateway,
        /// <summary>
        /// 503
        /// </summary>
        ServiceUnavailable
    }

}
