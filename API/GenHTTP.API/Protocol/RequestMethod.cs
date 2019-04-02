using System;
using System.Collections.Generic;
using System.Text;

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

    public class FlexibleRequestMethod
    {

        #region Get-/Setters

        public RequestMethod? KnownMethod { get; }

        public string RawMethod { get; }

        #endregion

        #region Initialization

        public FlexibleRequestMethod(string rawType)
        {
            RawMethod = rawType;
            
            if (Enum.TryParse<RequestMethod>(rawType, out var type))
            {
                KnownMethod = type;
            }
        }

        public FlexibleRequestMethod(RequestMethod type)
        {
            KnownMethod = type;
            RawMethod = type.ToString();
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
