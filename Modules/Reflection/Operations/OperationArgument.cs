using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Reflection.Operations;

/// <summary>
/// Defines where the engine will read the argument from
/// to obtain a value.
/// </summary>
public enum OperationArgumentSource
{

    /// <summary>
    /// The argument will be read from a path variable (e.g. "/users/:id").
    /// </summary>
    Path,

    /// <summary>
    /// The argument will either be read from the request query or form encoded data (e.g. "/users?id=1").
    /// </summary>
    Query,

    /// <summary>
    /// The argument will directly be read from the body of the request, with no deserialization applied.
    /// </summary>
    Body,

    /// <summary>
    /// The argument will be injected by a compatible <see cref="IParameterInjector" />.
    /// </summary>
    Injected,

    /// <summary>
    /// The argument will be deserialized from the body using a compatible <see cref="ISerializationFormat" />.
    /// </summary>
    Content,

    /// <summary>
    /// The argument re-presents the stream of the request body.
    /// </summary>
    Streamed

}

public sealed class OperationArgument
{

    #region Get-/Setters

    /// <summary>
    /// The name of the argument.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type as expected by the .NET method to be invoked by the operation.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Specifies how the argument can get read from the incoming request.
    /// </summary>
    public OperationArgumentSource Source { get; }

    #endregion

    #region Initialization

    public OperationArgument(string name, Type type, OperationArgumentSource source)
    {
        Name = name;
        Type = type;
        Source = source;
    }

    #endregion

}
