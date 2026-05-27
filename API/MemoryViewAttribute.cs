namespace GenHTTP.Api;

/// <summary>
/// Marks a <c>readonly partial struct</c> as a typed, memory-backed view type.
/// The source generator emits the backing field, constructors, public property,
/// <see cref="System.IEquatable{T}"/> implementation, and optional helpers into
/// a companion partial file.
/// </summary>
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
