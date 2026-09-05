using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>A provider that answers for more than one name.</summary>
public interface IHostCertificateProvider : ICertificateProvider
{
    IEnumerable<string> Hosts { get; }
}
