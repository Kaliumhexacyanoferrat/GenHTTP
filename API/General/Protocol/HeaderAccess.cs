namespace GenHTTP.Api.Protocol;

/// <summary>
/// Allows the user to specify whether access to the HTTP header is still needed
/// after the engine has started to read the body of the HTTP request. 
/// </summary>
public enum HeaderAccess
{
    
    /// <summary>
    /// Instructs the engine to allocate a copy of the HTTP header, therefore
    /// allowing handlers to still access header information after the
    /// engine has started to read the body.
    /// </summary>
    /// <remarks>
    /// Use this option for non-performance sensitive handlers.
    /// </remarks>
    Retain,
    
    /// <summary>
    /// Instructs the engine to return the memory used by the HTTP header back
    /// to the underlying mechanism (such as a pipe reader). Trying to access
    /// the request header will consequently result in an exception.
    /// </summary>
    /// <remarks>
    /// Use this option for high performance handlers that need zero allocation
    /// at all costs.
    /// </remarks>
    Release
    
}
