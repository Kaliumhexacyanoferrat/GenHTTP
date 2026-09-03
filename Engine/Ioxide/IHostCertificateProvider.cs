using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

public interface IHostCertificateProvider : ICertificateProvider
{
    IEnumerable<string> Hosts { get; }
}
