using System.Diagnostics;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp.Protocol;
using GenHTTP.Modules.Reflection;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Serves tools via the Streamable HTTP transport of the Model Context Protocol.
/// </summary>
/// <remarks>
/// Serves modern clients (2026-07-28) statelessly, validating the request metadata they
/// send with every request, and legacy clients (2025-03-26 to 2025-11-25) by answering
/// their initialize handshake without creating a session. Requests are always answered
/// with a single JSON object, never with an event stream.
/// </remarks>
internal sealed class ToolsHandler : IHandler
{
    private const string LoggerCategory = "GenHTTP.Modules.Mcp";

    private static readonly ByteString OriginHeader = new("Origin");

    private static readonly ByteString VersionHeader = new("MCP-Protocol-Version");

    private static readonly JsonDocumentOptions DocumentOptions = new() { MaxDepth = 64 };

    private static readonly JsonDocument EmptyArguments = JsonDocument.Parse("{}");

    private static readonly byte[] Capabilities = """{"tools":{}}"""u8.ToArray();

    private readonly MethodToolDefinition[] _definitions;

    private readonly MethodRegistry _registry;

    private readonly JsonSerializerOptions _serialization;

    private readonly JsonSerializerOptions _schemaOptions;

    private readonly RpcMessages _messages;

    private readonly string? _instructions;

    private readonly OriginPolicy _origins;

    private readonly Lock _preparation = new();

    private volatile ToolCatalog? _catalog;

    private ILogger? _logger;

    #region Initialization

    internal ToolsHandler(MethodToolDefinition[] definitions, MethodRegistry registry, ServerIdentity identity, string? instructions,
                          TimeSpan cacheDuration, OriginPolicy origins, JsonSerializerOptions serialization, JsonSerializerOptions schemaOptions)
    {
        _definitions = definitions;
        _registry = registry;
        _instructions = instructions;
        _origins = origins;
        _serialization = serialization;
        _schemaOptions = schemaOptions;

        _messages = new RpcMessages(identity.Name, identity.Version, identity.Title, cacheDuration);
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server)
    {
        _logger = server.Logging.CreateLogger(LoggerCategory);

        Prepare(server);

        return default;
    }

    private ToolCatalog Prepare(IServer server)
    {
        var catalog = _catalog;

        if (catalog != null)
        {
            return catalog;
        }

        lock (_preparation)
        {
            return _catalog ??= ToolCatalog.Create(server, _definitions, _registry, _serialization, _schemaOptions);
        }
    }

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var started = Stopwatch.GetTimestamp();

        var catalog = _catalog ?? Prepare(request.Server);

        if (request.Header.Method != RequestMethod.Post)
        {
            LogRejected(request, started, ResponseStatus.MethodNotAllowed, "Only POST is supported");

            return request.Respond()
                          .Status(ResponseStatus.MethodNotAllowed)
                          .Header("Allow", "POST")
                          .Build();
        }

        var origin = request.Header.Headers.GetEntry(OriginHeader);

        if (origin != null && !_origins.Allows(origin.Value.ToString()))
        {
            return Error(request, started, ResponseStatus.Forbidden, null, RpcErrors.InvalidRequest, $"Requests from origin '{origin}' are not allowed");
        }

        var body = request.GetBody();

        if (body == null)
        {
            return Error(request, started, ResponseStatus.BadRequest, null, RpcErrors.ParseError, "The request does not contain a JSON-RPC message");
        }

        var content = await body.AsMemoryAsync();

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(content, DocumentOptions);
        }
        catch (JsonException)
        {
            return Error(request, started, ResponseStatus.BadRequest, null, RpcErrors.ParseError, "The request body is not valid JSON");
        }

        using (document)
        {
            return await DispatchAsync(request, catalog, document.RootElement, started);
        }
    }

    private async ValueTask<IResponse> DispatchAsync(IRequest request, ToolCatalog catalog, JsonElement message, long started)
    {
        if (message.ValueKind != JsonValueKind.Object)
        {
            return Error(request, started, ResponseStatus.BadRequest, null, RpcErrors.InvalidRequest, "Expected a single JSON-RPC message object");
        }

        JsonElement? id = null;

        if (message.TryGetProperty("id"u8, out var givenId))
        {
            if (givenId.ValueKind is not (JsonValueKind.String or JsonValueKind.Number))
            {
                return Error(request, started, ResponseStatus.BadRequest, null, RpcErrors.InvalidRequest, "The id of a request needs to be a string or a number");
            }

            id = givenId;
        }

        if (!message.TryGetProperty("jsonrpc"u8, out var rpc) || rpc.ValueKind != JsonValueKind.String || !rpc.ValueEquals("2.0"u8))
        {
            return Error(request, started, ResponseStatus.BadRequest, id, RpcErrors.InvalidRequest, "Expected a JSON-RPC 2.0 message");
        }

        if (!message.TryGetProperty("method"u8, out var methodElement) || methodElement.ValueKind != JsonValueKind.String)
        {
            return Error(request, started, ResponseStatus.BadRequest, id, RpcErrors.InvalidRequest, "Expected a request or notification with a method");
        }

        var parameters = default(JsonElement);

        if (message.TryGetProperty("params"u8, out var givenParameters))
        {
            if (givenParameters.ValueKind != JsonValueKind.Object)
            {
                return Error(request, started, ResponseStatus.BadRequest, id, RpcErrors.InvalidRequest, "The parameters of a request need to be an object");
            }

            parameters = givenParameters;
        }

        var method = McpMethods.Identify(methodElement, out var methodName);

        if (id == null)
        {
            var logger = GetLogger(request);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.NotificationAccepted(methodName ?? methodElement.ToString());
            }

            return request.Respond()
                          .Status(ResponseStatus.Accepted)
                          .Build();
        }

        if (method == McpMethod.Initialize)
        {
            return Initialize(request, started, id.Value, parameters);
        }

        var header = request.Header.Headers.GetEntry(VersionHeader);

        string version;

        if (header != null)
        {
            var known = McpVersions.Find(header.Value);

            if (known == null)
            {
                var requested = header.Value.ToString();

                LogRejected(request, started, ResponseStatus.BadRequest, $"Unsupported protocol version '{requested}'");

                return Respond(request, ResponseStatus.BadRequest, RpcMessages.UnsupportedVersion(id.Value, requested));
            }

            version = known;
        }
        else if (RequestMetadata.TryGetMeta(parameters, out var meta) && meta.TryGetProperty(MetaKeys.ProtocolVersion, out _))
        {
            return Error(request, started, ResponseStatus.BadRequest, id, RpcErrors.HeaderMismatch, "The MCP-Protocol-Version header is required");
        }
        else
        {
            version = McpVersions.Unspecified;
        }

        var modern = McpVersions.IsModern(version);

        if (modern)
        {
            var problem = RequestMetadata.Validate(request, version, method, methodElement, parameters, out var code);

            if (problem != null)
            {
                return Error(request, started, ResponseStatus.BadRequest, id, code, problem);
            }
        }

        switch (method)
        {
            case McpMethod.CallTool:
                return await CallToolAsync(request, catalog, started, id.Value, parameters, modern);

            case McpMethod.ListTools:
                return ListTools(request, catalog, started, id.Value, parameters, version);

            case McpMethod.Discover:
                return Discover(request, started, id.Value, parameters, version);

            case McpMethod.Ping:
                return Complete(request, started, "ping", RpcMessages.BeginResult(id.Value), version, cacheable: false);

            case McpMethod.ListResources:
                return EmptyList(request, started, id.Value, "resources/list", "resources"u8, version);

            case McpMethod.ListResourceTemplates:
                return EmptyList(request, started, id.Value, "resources/templates/list", "resourceTemplates"u8, version);

            case McpMethod.ListPrompts:
                return EmptyList(request, started, id.Value, "prompts/list", "prompts"u8, version);

            default:
                return Error(request, started, modern ? ResponseStatus.NotFound : ResponseStatus.Ok, id, RpcErrors.MethodNotFound, $"Method '{methodElement}' is not supported");
        }
    }

    private IResponse Initialize(IRequest request, long started, JsonElement id, JsonElement parameters)
    {
        var version = McpVersions.Negotiate(GetString(parameters, "protocolVersion"u8));

        var buffer = RpcMessages.BeginResult(id);
        var w = buffer.Writer;

        w.WriteString("protocolVersion"u8, version);

        w.WritePropertyName("capabilities"u8);
        w.WriteRawValue(Capabilities, skipInputValidation: true);

        w.WritePropertyName("serverInfo"u8);
        w.WriteRawValue(_messages.ServerInfo, skipInputValidation: true);

        WriteInstructions(w);

        var response = Respond(request, ResponseStatus.Ok, _messages.EndResult(buffer, modern: false, cacheable: false));

        LogClient(request, started, "initialize", parameters.ValueKind == JsonValueKind.Object && parameters.TryGetProperty("clientInfo"u8, out var client) ? client : default, version);

        return response;
    }

    private IResponse Discover(IRequest request, long started, JsonElement id, JsonElement parameters, string version)
    {
        var buffer = RpcMessages.BeginResult(id);
        var w = buffer.Writer;

        RpcMessages.WriteSupportedVersions(w, "supportedVersions"u8);

        w.WritePropertyName("capabilities"u8);
        w.WriteRawValue(Capabilities, skipInputValidation: true);

        WriteInstructions(w);

        var response = Respond(request, ResponseStatus.Ok, _messages.EndResult(buffer, McpVersions.IsModern(version), cacheable: true));

        LogClient(request, started, "server/discover", RequestMetadata.TryGetMeta(parameters, out var meta) && meta.TryGetProperty(MetaKeys.ClientInfo, out var client) ? client : default, version);

        return response;
    }

    private IResponse ListTools(IRequest request, ToolCatalog catalog, long started, JsonElement id, JsonElement parameters, string version)
    {
        if (parameters.ValueKind == JsonValueKind.Object && parameters.TryGetProperty("cursor"u8, out var cursor) && cursor.ValueKind != JsonValueKind.Null)
        {
            return Error(request, started, ResponseStatus.Ok, id, RpcErrors.InvalidParams, "The given cursor is not valid");
        }

        var buffer = RpcMessages.BeginResult(id);

        buffer.Writer.WritePropertyName("tools"u8);
        buffer.Writer.WriteRawValue(catalog.List, skipInputValidation: true);

        return Complete(request, started, "tools/list", buffer, version, cacheable: true);
    }

    private IResponse EmptyList(IRequest request, long started, JsonElement id, string method, ReadOnlySpan<byte> property, string version)
    {
        var buffer = RpcMessages.BeginResult(id);

        buffer.Writer.WriteStartArray(property);
        buffer.Writer.WriteEndArray();

        return Complete(request, started, method, buffer, version, cacheable: true);
    }

    private async ValueTask<IResponse> CallToolAsync(IRequest request, ToolCatalog catalog, long started, JsonElement id, JsonElement parameters, bool modern)
    {
        if (parameters.ValueKind != JsonValueKind.Object || !parameters.TryGetProperty("name"u8, out var name) || name.ValueKind != JsonValueKind.String)
        {
            return Error(request, started, ResponseStatus.Ok, id, RpcErrors.InvalidParams, "The name of the tool to call is required");
        }

        var tool = catalog.Find(name);

        if (tool == null)
        {
            return Error(request, started, ResponseStatus.Ok, id, RpcErrors.InvalidParams, $"Unknown tool: {name}");
        }

        var arguments = EmptyArguments.RootElement;

        if (parameters.TryGetProperty("arguments"u8, out var givenArguments) && givenArguments.ValueKind != JsonValueKind.Null)
        {
            if (givenArguments.ValueKind != JsonValueKind.Object)
            {
                return Error(request, started, ResponseStatus.Ok, id, RpcErrors.InvalidParams, "The arguments of a tool call need to be an object");
            }

            arguments = givenArguments;
        }

        var outcome = await tool.Invoker.InvokeAsync(arguments, new ToolContext(request, this));

        try
        {
            var buffer = RpcMessages.BeginResult(id);

            ToolResultWriter.Write(buffer.Writer, outcome, modern);

            var response = Respond(request, ResponseStatus.Ok, _messages.EndResult(buffer, modern, cacheable: false));

            LogOutcome(request, started, tool.Name, outcome);

            return response;
        }
        finally
        {
            outcome.Structured?.Dispose();
        }
    }

    #endregion

    #region Helpers

    private IResponse Complete(IRequest request, long started, string method, JsonBuffer buffer, string version, bool cacheable)
    {
        var response = Respond(request, ResponseStatus.Ok, _messages.EndResult(buffer, McpVersions.IsModern(version), cacheable));

        var logger = GetLogger(request);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.OperationHandled(method, Elapsed(started), version);
        }

        return response;
    }

    private IResponse Error(IRequest request, long started, ResponseStatus status, JsonElement? id, int code, string message)
    {
        LogRejected(request, started, status, message);

        return Respond(request, status, RpcMessages.Error(id, code, message));
    }

    private void WriteInstructions(Utf8JsonWriter writer)
    {
        if (_instructions != null)
        {
            writer.WriteString("instructions"u8, _instructions);
        }
    }

    private static IResponse Respond(IRequest request, ResponseStatus status, byte[] content)
        => request.Respond()
                  .Status(status)
                  .Content(new RpcContent(content))
                  .Build();

    private static string? GetString(JsonElement parameters, ReadOnlySpan<byte> property)
        => parameters.ValueKind == JsonValueKind.Object && parameters.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private ILogger GetLogger(IRequest request) => _logger ??= request.Server.Logging.CreateLogger(LoggerCategory);

    private static double Elapsed(long started) => Stopwatch.GetElapsedTime(started).TotalMilliseconds;

    private void LogClient(IRequest request, long started, string method, JsonElement client, string version)
    {
        var logger = GetLogger(request);

        if (!logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        var name = client.ValueKind == JsonValueKind.Object ? GetString(client, "name"u8) : null;
        var clientVersion = client.ValueKind == JsonValueKind.Object ? GetString(client, "version"u8) : null;

        logger.ClientHandled(method, name ?? "unknown client", clientVersion ?? string.Empty, Elapsed(started), version);
    }

    private void LogOutcome(IRequest request, long started, string tool, in ToolOutcome outcome)
    {
        var logger = GetLogger(request);

        if (outcome.Crash != null)
        {
            logger.ToolCrashed(outcome.Crash, tool, Elapsed(started));
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            if (outcome.Result.IsError)
            {
                logger.ToolFailed(tool, Elapsed(started), outcome.Result.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? "error result");
            }
            else
            {
                logger.ToolSucceeded(tool, Elapsed(started));
            }
        }
    }

    private void LogRejected(IRequest request, long started, ResponseStatus status, string reason)
    {
        var logger = GetLogger(request);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.RequestRejected((int)status, Elapsed(started), reason);
        }
    }

    #endregion

}
