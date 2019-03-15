using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Http;
using GenHTTP.Api.SessionManagement;

namespace GenHTTP.Api.Caching
{

    /// <summary>
    /// Defines the signature of a method, used to determine, whether
    /// the cache should send a cached response to the client or not.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="info">Information about the client's status</param>
    /// <returns>true, if the cache should send a cached response</returns>
    public delegate bool CheckApplicability(IHttpRequest request, AuthorizationInfo info);

}
