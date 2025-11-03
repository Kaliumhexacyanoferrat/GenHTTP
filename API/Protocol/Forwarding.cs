using System.Net.Sockets;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Stores information how a request has been proxied
/// to the server.
/// </summary>
public record Forwarding(string? For, string? Host, ClientProtocol? Protocol)
{

    public static Forwarding From(IClientConnection connection)
    {
        var forString = connection.IPAddress.AddressFamily == AddressFamily.InterNetworkV6
            ? $"[{connection.IPAddress}]"
            : connection.IPAddress.ToString();

        return new Forwarding(forString, connection.Host, connection.Protocol);
    }

    public string Format()
    {
        var result = new List<string>(3);

        if (For is not null)
        {
            result.Add($"for={For}");
        }

        if (Host is not null)
        {
            result.Add($"host={Host}");
        }

        if (Protocol is not null)
        {
            result.Add($"proto={Protocol?.ToString() ?? "http"}");
        }

        return string.Join(", ", result);
    }

}
