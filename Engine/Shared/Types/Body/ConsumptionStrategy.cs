namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Used internally to track the intention of the user on
/// how to read the body.
/// </summary>
internal enum ConsumptionStrategy
{
    
    /// <summary>
    /// The user would like to receive the body as a stream.
    /// </summary>
    Stream,
    
    /// <summary>
    /// The user requested the body as a memory view.
    /// </summary>
    Memory
    
}