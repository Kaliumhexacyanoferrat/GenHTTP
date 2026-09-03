using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

public interface IFileCertificateProvider : ICertificateProvider
{
    CertificateFiles? ProvideFiles(string? host);
}

public sealed record CertificateFiles(string Certificate, string Key);
