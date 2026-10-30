namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Hints describing the effects of a tool, as declared via <see cref="ToolAttribute" />.
/// </summary>
/// <remarks>
/// Hints that are not set are not sent, so clients assume the defaults of the specification.
/// </remarks>
internal sealed record ToolHints(bool? ReadOnly, bool? Destructive, bool? Idempotent, bool? OpenWorld);
