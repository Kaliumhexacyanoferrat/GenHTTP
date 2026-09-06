using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>A provider that can also name its certificate as files, which is the only form HTTP/3 takes.</summary>
public interface IFileCertificateProvider : ICertificateProvider
{
    // The certificate and key as PEM paths, or null where only the in-memory form exists.
    CertificateFiles? ProvideFiles(string? host);
}

/// <summary>A pair of paths: one PEM file holding the certificate chain, one holding its key.</summary>
public sealed record CertificateFiles(string Certificate, string Key);
