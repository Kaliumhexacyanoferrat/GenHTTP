﻿using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal.Infrastructure;

namespace GenHTTP.Engine.Internal;

/// <summary>
/// Allows to create server instances.
/// </summary>
public static class Server
{

    /// <summary>
    /// Create a new, configurable server instance with
    /// default values.
    /// </summary>
    /// <returns>The builder to create the instance</returns>
    public static IServerBuilder Create() => new ThreadedServerBuilder();
}
