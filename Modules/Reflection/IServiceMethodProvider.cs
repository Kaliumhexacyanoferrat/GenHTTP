﻿namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Implemented by handlers that use the <see cref="MethodCollection" /> handler
/// for response generation. Allows logic interested in generically analyzing
/// such handlers (e.g. the OpenAPI concern) to stay loosely coupled.
/// </summary>
public interface IServiceMethodProvider
{

    /// <summary>
    /// The method collection actually serving requests.
    /// </summary>
    MethodCollection Methods { get; }

}
