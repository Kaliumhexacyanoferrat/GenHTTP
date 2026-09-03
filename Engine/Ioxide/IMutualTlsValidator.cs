using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

public interface IMutualTlsValidator : ICertificateValidator
{
    string? ClientCaPath => null;

    string? ClientCaPem => null;
}
