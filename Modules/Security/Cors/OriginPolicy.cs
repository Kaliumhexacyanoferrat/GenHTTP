using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Cors
{

    public class OriginPolicy
    {

        #region Get-/Setters

        /// <summary>
        /// The HTTP methods the client is allowed to access (any, if not given).
        /// </summary>
        public List<FlexibleRequestMethod>? AllowedMethods { get; }

        /// <summary>
        /// The headers a client may send to the server (any, if not given).
        /// </summary>
        public List<string>? AllowedHeaders { get; }

        /// <summary>
        /// The headers that will be accessible by the client (any, if not given).
        /// </summary>
        public List<string>? ExposedHeaders { get; }

        /// <summary>
        /// Whether the client is allowed to read credentials from the request.
        /// </summary>
        public bool AllowCredentials { get; }

        /// <summary>
        /// The duration in seconds this policy is valid for.
        /// </summary>
        public uint MaxAge { get; }

        #endregion

        #region Initialization

        public OriginPolicy(List<FlexibleRequestMethod>? allowedMethods, List<string>? allowedHeaders,
                            List<string>? exposedHeaders, bool allowCredentials, uint maxAge)
        {
            AllowedMethods = allowedMethods;
            AllowedHeaders = allowedHeaders;

            ExposedHeaders = exposedHeaders;

            AllowCredentials = allowCredentials;

            MaxAge = maxAge;
        }

        #endregion

    }

}
