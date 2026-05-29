namespace GenHTTP.Api;

/// <summary>
/// Marks a struct as a typed view onto a chunk of memory
/// read from the request buffer, such as the protocol
/// version, the request path or the content type of the request body.
/// </summary>
/// <remarks>>
/// Marking a struct with this attribute will automatically generate
/// fields to hold the memory reference, constructors, equality methods
/// and hashing functions.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class MemoryViewAttribute : Attribute;
