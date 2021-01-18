﻿using GenHTTP.Modules.ClientCaching.Policy;
using GenHTTP.Modules.ClientCaching.Validation;

namespace GenHTTP.Modules.ClientCaching
{

    public static class ClientCache
    {

        /// <summary>
        /// Creates a concern that will tag content generated by the server
        /// so the clients can check, whether new content is available
        /// on the server. 
        /// </summary>
        public static CacheValidationBuilder Validation() => new CacheValidationBuilder();

        /// <summary>
        /// Creates a policy defining how the client should cache
        /// the content generated by the server.
        /// </summary>
        public static CachePolicyBuilder Policy() => new CachePolicyBuilder();

    }

}
