using GenHTTP.Api.Content;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;

namespace GenHTTP.Modules.Reflection.Operations;

/// <summary>
/// Specifies how a result returned by an operation should get converted
/// into a HTTP response.
/// </summary>
public enum OperationResultSink
{

    /// <summary>
    /// The value should be formatted using a compatible <see cref="IFormatter" />, without serialization being applied.
    /// </summary>
    Formatter,

    /// <summary>
    /// The value should get serialized to the response body, using a compatible <see cref="ISerializationFormat" />.
    /// </summary>
    Serializer,

    /// <summary>
    /// The value returned by the operation is a framework type that can generate a response on its own, such
    /// as an <see cref="IHandler" />.
    /// </summary>
    Dynamic,

    /// <summary>
    /// The value represents a stream that should directly be passed to the client.
    /// </summary>
    Stream,

    /// <summary>
    /// The operation does not return any value, resulting in a HTTP 204 response.
    /// </summary>
    None

}

public class OperationResult
{

    #region Get-/Setters

    /// <summary>
    /// The sink to be used to generate a HTTP response.
    /// </summary>
    public OperationResultSink Sink { get; }

    /// <summary>
    /// The type of the result as declared by the .NET method.
    /// </summary>
    public Type Type { get; }

    #endregion

    #region Initialization

    public OperationResult(Type type, OperationResultSink sink)
    {
        Type = type;
        Sink = sink;
    }

    #endregion

}
