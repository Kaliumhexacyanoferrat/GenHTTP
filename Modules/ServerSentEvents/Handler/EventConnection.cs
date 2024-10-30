using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public sealed class EventConnection : IEventConnection
{
    private const string NL = "\n";

    private static readonly Encoding Encoding = Encoding.UTF8;

    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    #region Get-/Setters

    public IRequest Request { get; }

    public string? LastEventId { get; }

    public bool Connected { get; private set; }

    private FormatterRegistry Formatters { get; }

    private Stream Target { get; }

    #endregion

    #region Initialization

    public EventConnection(IRequest request, string? lastEventId, Stream target, FormatterRegistry formatters)
    {
        Request = request;
        LastEventId = lastEventId;
        Target = target;

        Formatters = formatters;

        Connected = true;
    }

    #endregion

    #region Functionality

    public ValueTask<bool> CommentAsync(string comment) => SendAsync($": {comment}{NL}{NL}");

    public async ValueTask<bool> DataAsync(string data, string? eventType = null, string? eventId = null)
    {
        if (eventType != null)
        {
            await SendAsync($"event: {eventType}{NL}");
        }

        if (eventId != null)
        {
            await SendAsync($"id: {eventId}{NL}");
        }

        return await SendAsync($"data: {data}{NL}{NL}");
    }

    public async ValueTask<bool> DataAsync<T>(T data, string? eventType = null, string? eventId = null)
    {
        if (data is null)
        {
            return await DataAsync(string.Empty, eventType, eventId);
        }

        var type = data.GetType();

        if (Formatters.CanHandle(type))
        {
            return await DataAsync(Formatters.Write(data, type) ?? string.Empty, eventType, eventId);
        }

        var buffer = new MemoryStream();

        await JsonSerializer.SerializeAsync(buffer, data, SerializationOptions);

        return await DataAsync(Encoding.GetString(buffer.ToArray()));
    }

    public async ValueTask<bool> RetryAsync(int milliseconds)
    {
        if (await SendAsync($"retry {milliseconds}{NL}{NL}"))
        {
            Connected = false;
            return true;
        }

        return false;
    }

    private async ValueTask<bool> SendAsync(string message)
    {
        if (!Connected) return false;

        var length = Encoding.GetByteCount(message);

        var buffer = Pool.Rent(length);

        try
        {
            Encoding.GetBytes(message, 0, message.Length, buffer, 0);

            await Target.WriteAsync(buffer.AsMemory(0, length));

            return true;
        }
        catch (IOException)
        {
            Connected = false;
            return false;
        }
        finally
        {
            Pool.Return(buffer);
        }
    }

    #endregion

}
