using System.Net;
using System.Security.Authentication;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure;

internal record ServerConfiguration(bool DevelopmentMode, IEnumerable<EndPointConfiguration> EndPoints,
    NetworkConfiguration Network);

internal record NetworkConfiguration(TimeSpan RequestReadTimeout, uint RequestMemoryLimit,
    uint TransferBufferSize, ushort Backlog);

internal record EndPointConfiguration(IPAddress Address, ushort Port, SecurityConfiguration? Security);

internal record SecurityConfiguration(ICertificateProvider Certificate, SslProtocols Protocols);
