using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>A validator that also names the anchors client certificates are checked against.</summary>
public interface IMutualTlsValidator : ICertificateValidator
{
    string? ClientCaPath => null;

    string? ClientCaPem => null;
}
