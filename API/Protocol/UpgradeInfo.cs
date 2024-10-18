using System.Net.Sockets;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Returned when upgrading a connection.
/// </summary>
/// <param name="Socket">The raw socket the current client is connected to</param>
/// <param name="Response">The response to return so that the server will ignore the connection further on</param>
public record UpgradeInfo(Socket Socket, IResponse Response);
