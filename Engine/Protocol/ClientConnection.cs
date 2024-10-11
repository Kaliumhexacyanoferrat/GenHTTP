using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol;

internal record ClientConnection(IPAddress IPAddress, ClientProtocol? Protocol, string? Host) : IClientConnection;
