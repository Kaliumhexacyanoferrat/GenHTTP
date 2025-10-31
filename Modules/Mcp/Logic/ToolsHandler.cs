using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp.Types;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Websockets;

using NJsonSchema;

namespace GenHTTP.Modules.Mcp.Logic;

public class ToolsHandler : IHandler
{
    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly IHandler _Websocket;

    private readonly Dictionary<string, ITool> _Tools;

    #region Initialization

    public ToolsHandler(List<ITool> tools)
    {
        _Tools = tools.ToDictionary(t => t.Name, t => t);

        _Websocket = Websocket.Create()
                              .OnMessage(DispatchMessage)
                              .Build();
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => _Websocket.HandleAsync(request);

    private async Task DispatchMessage(IWebsocketConnection connection, string message)
    {
        var request = JsonSerializer.Deserialize<JsonRpcRequest>(message, SerializationOptions);

        if (request == null)
        {
            throw new ArgumentException("Unable read JsonRpc frame from message");
        }

        switch (request.Method)
        {
            case "tools/list":
                {
                    var response = new JsonRpcResponse<ToolList>
                    {
                        Id = request.Id,
                        Result = ListTools()
                    };

                    await connection.SendAsync(JsonSerializer.Serialize(response));
                    break;
                }
            case "tools/call":
                {
                    var arguments = request.Params?.Deserialize<ToolCallParams>();

                    if (arguments?.Name is not null)
                    {
                        if (_Tools.TryGetValue(arguments.Name, out var tool))
                        {
                            try
                            {
                                var result = await CallTool(tool, arguments.Arguments);

                                var response = new JsonRpcResponse<object?>
                                {
                                    Id = request.Id,
                                    Result = result
                                };

                                await connection.SendAsync(JsonSerializer.Serialize(response));
                            }
                            catch (Exception e)
                            {
                                await SendError(connection, 22, $"Error while executing tool '{arguments.Name}': {e.Message}");
                            }
                        }
                        else
                        {
                            await SendError(connection, 20, $"Unrecognized tool '{arguments.Name}'");
                        }
                    }
                    else
                    {
                        await SendError(connection, 21, "Unable to read tool from request");
                    }

                    break;
                }
            default:
                {
                    await SendError(connection, 10, $"Unsupported method '{request.Method}'");
                    break;
                }
        }
    }

    private ToolList ListTools()
    {
        var descriptions = new List<ToolInfo>();

        foreach (var tool in _Tools.Values)
        {
            descriptions.Add(new ToolInfo
            {
                Name = tool.Name,
                Description = tool.Description,
                InputSchema = JsonSchema.FromType(tool.InputType),
                OutputSchema = JsonSchema.FromType(tool.OutputType)
            });
        }

        return new ToolList()
        {
            Tools = descriptions
        };
    }

    private static async ValueTask<object?> CallTool(ITool tool, JsonElement? input)
    {
        var argument = input?.Deserialize(tool.InputType);

        var result = tool.CallUntyped(argument!);

        return await MethodHandler.UnwrapAsync(result);
    }

    #endregion

    #region Helpers

    private static async ValueTask SendError(IWebsocketConnection connection, int code, string message)
    {
        var response = new JsonRpcError()
        {
            Code = code,
            Message = message
        };

        await connection.SendAsync(JsonSerializer.Serialize(response));
    }

    #endregion

}
