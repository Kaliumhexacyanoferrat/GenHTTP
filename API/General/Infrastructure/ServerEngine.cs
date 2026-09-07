namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Identifies the engine used by a server instance.
/// </summary>
/// <remarks>
/// The engine field allows modules to be optimized for a specific
/// engine implementation. In general, modules must provide their
/// functionality independently of the underlying engine and must not
/// throw exceptions for engines they do not know. Therefore,
/// it is recommended not to rely on this field in business logic.
/// </remarks>
public enum ServerEngine
{
    
    /// <summary>
    /// The internal engine of GenHTTP.
    /// </summary>
    Internal,
    
    /// <summary>
    /// Engine based on Microsoft's ASP.NET Core.
    /// </summary>
    Kestrel,
    
    /// <summary>
    /// High performance Linux engine based on io_uring.
    /// </summary>
    Ioxide,
    
    /// <summary>
    /// A custom implementation that is not shipped with the default SDK.
    /// </summary>
    Custom
    
}
