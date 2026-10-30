using System.Text.Json;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Mcp.Protocol;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// The tools offered by a handler, with their definitions encoded once
/// so the tool list can be sent without serializing it again.
/// </summary>
internal sealed class ToolCatalog
{

    #region Get-/Setters

    internal byte[] List { get; }

    private RegisteredTool[] Tools { get; }

    #endregion

    #region Initialization

    private ToolCatalog(RegisteredTool[] tools, byte[] list)
    {
        Tools = tools;
        List = list;
    }

    internal static ToolCatalog Create(IServer server, MethodToolDefinition[] definitions, MethodRegistry registry, JsonSerializerOptions serialization, JsonSerializerOptions schemaOptions)
    {
        var tools = new RegisteredTool[definitions.Length];

        for (var i = 0; i < definitions.Length; i++)
        {
            tools[i] = MethodToolFactory.Create(server, definitions[i], registry, serialization, schemaOptions);
        }

        using var buffer = JsonBuffer.Rent();

        buffer.Writer.WriteStartArray();

        foreach (var tool in tools)
        {
            tool.WriteDefinition(buffer.Writer);
        }

        buffer.Writer.WriteEndArray();

        return new ToolCatalog(tools, buffer.ToArray());
    }

    #endregion

    #region Functionality

    internal RegisteredTool? Find(JsonElement name)
    {
        foreach (var tool in Tools)
        {
            if (name.ValueEquals(tool.EncodedName))
            {
                return tool;
            }
        }

        return null;
    }

    #endregion

}
