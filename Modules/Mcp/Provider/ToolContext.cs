using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// The request a tool is invoked for and the handler serving it.
/// </summary>
internal readonly record struct ToolContext(IRequest Request, IHandler Handler);
