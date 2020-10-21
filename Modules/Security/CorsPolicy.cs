using System.Collections.Generic;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Security.Cors;

namespace GenHTTP.Modules.Security
{

    public static class CorsPolicy
    {

        /// <summary>
        /// Creates a policy that does not restrict browsers from interacting
        /// with the requested resources.
        /// </summary>
        public static CorsPolicyBuilder Permissive() => new CorsPolicyBuilder().Default(new OriginPolicy(null, null, null, true, 86400));

        /// <summary>
        /// Creates a policy that denies access to resources by browsers.
        /// </summary>
        /// <remarks>
        /// You may add more permissive policies for specific origins.
        /// </remarks>
        public static CorsPolicyBuilder Restrictive()
        {
            return new CorsPolicyBuilder();
        }

    }

}
