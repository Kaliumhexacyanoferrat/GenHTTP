using System.Reflection;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace GenHTTP.Modules.Mcp.Schema;

/// <summary>
/// A parameter of a tool method that is read from the arguments sent by the client.
/// </summary>
internal sealed record ToolArgument(string Name, ParameterInfo Parameter, bool Required, object? DefaultValue, JsonTypeInfo TypeInfo, IArgumentValidator? Validator)
{

    internal Type Type => Parameter.ParameterType;

    internal byte[] EncodedName { get; } = Encoding.UTF8.GetBytes(Name);

}
