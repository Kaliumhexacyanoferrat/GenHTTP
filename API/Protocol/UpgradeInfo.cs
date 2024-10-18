using System.Net.Sockets;

namespace GenHTTP.Api.Protocol;

public record UpgradeInfo(Socket Socket, IResponse Response);
