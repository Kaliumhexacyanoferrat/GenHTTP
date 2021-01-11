using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.ServerCaching.Provider
{

    public static class CacheKey
    {

        public static string GetKey(this IRequest request)
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + (ulong)request.Target.Path.ToString().GetHashCode();

                foreach (var arg in request.Query)
                {
                    hash = hash * 23 + (ulong)arg.Key.GetHashCode();
                    hash = hash * 23 + (ulong)arg.Value.GetHashCode();
                }

                if (request.Host != null)
                {
                    hash = hash * 23 + (ulong)request.Host.GetHashCode();
                }

                return hash.ToString();
            }
        }

        public static string GetVariationKey(this Dictionary<string, string>? variations)
        {
            unchecked
            {
                ulong hash = 17;

                if (variations != null)
                {
                    foreach (var arg in variations)
                    {
                        hash = hash * 23 + (ulong)arg.Key.GetHashCode();
                        hash = hash * 23 + (ulong)arg.Value.GetHashCode();
                    }
                }

                return hash.ToString();
            }
        }

    }

}
