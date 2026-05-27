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
internal sealed class MemoryViewAttribute : Attribute
{

    /// <summary>Name of the private <c>ReadOnlyMemory&lt;byte&gt;</c> backing field.</summary>
    public string FieldName { get; }

    /// <summary>Name of the public property that exposes the backing memory. Defaults to <c>"Value"</c>.</summary>
    public string PropertyName { get; init; } = "Value";

    /// <summary>
    /// Generate a <c>ToString()</c> override and a <c>[DebuggerDisplay]</c> helper.
    /// Set to <c>false</c> for types that provide their own string representation.
    /// </summary>
    public bool GenerateToString { get; init; } = true;

    public MemoryViewAttribute(string fieldName = "_value")
    {
        FieldName = fieldName;
    }

}
