using System.Net;
using System.Security.Authentication;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Shared.Infrastructure;

public record ServerConfiguration(bool DevelopmentMode, IEnumerable<EndPointConfiguration> EndPoints,
    NetworkConfiguration Network);

public record NetworkConfiguration(TimeSpan RequestReadTimeout, uint RequestMemoryLimit,
    uint TransferBufferSize, ushort Backlog);

public record EndPointConfiguration(IReadOnlyList<IPAddress>? Addresses, ushort Port, SecurityConfiguration? Security, bool EnableQuic);

public record SecurityConfiguration(ICertificateProvider CertificateProvider, SslProtocols Protocols, ICertificateValidator? CertificateValidator);
