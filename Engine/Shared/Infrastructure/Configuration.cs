using System.Net;
using System.Security.Authentication;

using GenHTTP.Api.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Shared.Infrastructure;

public record ServerConfiguration(bool DevelopmentMode, IEnumerable<EndPointConfiguration> EndPoints, ILoggerFactory Logging);

public record EndPointConfiguration(IPAddress? Address, ushort Port, HttpProtocols Protocols, bool DualStack, SecurityConfiguration? Security);

public record SecurityConfiguration(ICertificateProvider CertificateProvider, SslProtocols Protocols, ICertificateValidator? CertificateValidator);
