using System.Reflection;

namespace GenHTTP.Modules.Mcp.Provider;

internal sealed record ServerIdentity(string Name, string Version, string? Title)
{

    internal static ServerIdentity Default()
    {
        var assembly = Assembly.GetEntryAssembly()?.GetName();

        return new ServerIdentity(assembly?.Name ?? "GenHTTP", assembly?.Version?.ToString(3) ?? "1.0.0", null);
    }

}
