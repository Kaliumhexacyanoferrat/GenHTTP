using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Providers;

public sealed class StrictTransportConcern : IConcern
{
    private static readonly ReadOnlyMemory<byte> StrictTransportHeader = "Strict-Transport-Security"u8.ToArray();

    #region Get-/Setters

    public IHandler Content { get; }

    public StrictTransportPolicy Policy { get; }

    private ReadOnlyMemory<byte> HeaderValue { get; }

    #endregion

    #region Initialization

    public StrictTransportConcern(IHandler content, StrictTransportPolicy policy)
    {
        Content = content;

        Policy = policy;
        HeaderValue = GetPolicyHeader();
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await Content.HandleAsync(request);

        if (response is not null)
        {
            if (request.EndPoint.Secure)
            {
                var existing = response.Headers.GetEntry(StrictTransportHeader);

                if (existing == null)
                {
                    response.Rebuild().Header(StrictTransportHeader, HeaderValue);
                }
            }
        }

        return response;
    }

    private ReadOnlyMemory<byte> GetPolicyHeader()
    {
        var seconds = (int)Policy.MaximumAge.TotalSeconds;

        var result = $"max-age={seconds}";

        if (Policy.IncludeSubdomains)
        {
            result += "; includeSubDomains";
        }

        if (Policy.Preload)
        {
            result += "; preload";
        }

        return Encoding.ASCII.GetBytes(result);
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
