using System.Net;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public record ClientConnection(IPAddress? IpAddress, ClientProtocol? Protocol, string? Host, X509Certificate? Certificate) : IClientConnection;
