using GenHTTP.Modules.Mcp.Provider;

namespace GenHTTP.Modules.Mcp;

/// <summary>
/// Provides tools to AI agents such as Claude via the Model Context Protocol (MCP).
/// </summary>
/// <example>
/// <code>
/// var mcp = Tools.Create()
///                .Server("weather", "1.0.0")
///                .Add&lt;WeatherTools&gt;()
///                .Add("add", "Adds two numbers", (int a, int b) => a + b);
///
/// var app = Layout.Create()
///                 .Add("mcp", mcp);
/// </code>
/// </example>
public static class Tools
{

    /// <summary>
    /// Creates a new endpoint to add tools to.
    /// </summary>
    public static ToolsHandlerBuilder Create() => new();

}
