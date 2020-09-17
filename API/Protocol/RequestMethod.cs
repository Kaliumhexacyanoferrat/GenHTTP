﻿using System;

namespace GenHTTP.Api.Protocol
{

    public enum RequestMethod
    {
        GET,
        HEAD,
        POST,
        PUT,
        PATCH,
        DELETE,
        OPTIONS,
        PROPFIND,
        PROPPATCH,
        MKCOL,
        COPY,
        MOVE,
        LOCK,
        UNLOCK
    }

    /// <summary>
    /// The kind of request sent by the client.
    /// </summary>
    public struct FlexibleRequestMethod
    {

        #region Get-/Setters

        /// <summary>
        /// The known method of the request, if any.
        /// </summary>
        public RequestMethod? KnownMethod { get; }

        /// <summary>
        /// The raw method of the request.
        /// </summary>
        public string RawMethod { get; }

        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Creates a new request method instance from a known type.
        /// </summary>
        /// <param name="method">The known type to be used</param>
        public FlexibleRequestMethod(RequestMethod method) 
        {
            KnownMethod = method;
            RawMethod = method.ToString();
        }

        /// <summary>
        /// Create a new request method instance.
        /// </summary>
        /// <param name="rawType">The raw type transmitted by the client</param>
        public FlexibleRequestMethod(string rawType)
        {
            RawMethod = rawType;

            if (Enum.TryParse<RequestMethod>(rawType, out var type))
            {
                KnownMethod = type;
            }
            else
            {
                KnownMethod = null;
            }
        }

        #endregion

        #region Convenience

        public static bool operator ==(FlexibleRequestMethod method, RequestMethod knownMethod) => method.KnownMethod == knownMethod;

        public static bool operator !=(FlexibleRequestMethod method, RequestMethod knownMethod) => method.KnownMethod != knownMethod;

        public static bool operator ==(FlexibleRequestMethod method, string rawMethod) => method.RawMethod == rawMethod;

        public static bool operator !=(FlexibleRequestMethod method, string rawMethod) => method.RawMethod != rawMethod;

        public override bool Equals(object obj) => obj is FlexibleRequestMethod method && RawMethod == method.RawMethod;

        public override int GetHashCode() => RawMethod.GetHashCode();

        #endregion

    }

}
