using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Multi;

public sealed class MultiAuthenticationConcern : IConcern
{
    #region Get-/Setters

    public IHandler Content { get; }

    private readonly IConcern[] _delegatingConcerns;

    #endregion

    #region Initialization

    public MultiAuthenticationConcern(IHandler content, IConcern[] delegatingConcerns)
    {
        Content = content;
        _delegatingConcerns = delegatingConcerns ?? [];
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        ResponseOrException? lastResponse = null;
        foreach (var concern in _delegatingConcerns)
        {
            lastResponse?.Dispose();

            try
            {
                lastResponse = new(await concern.HandleAsync(request));
            }
            catch (ProviderException e)
            {
                lastResponse = new(Exception: e);
            }

            if (lastResponse.Status != ResponseStatus.Unauthorized)
            {
                return lastResponse.Get();
            }
        }

        return lastResponse?.Get() ??
                     request.Respond()
                            .Status(ResponseStatus.Unauthorized)
                            .Build();
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

    #region Helper structure

    private sealed record ResponseOrException(IResponse? Response = null, ProviderException? Exception = null) : IDisposable
    { 
        public IResponse? Get()
        {
            if (Exception != null)
            {
                throw Exception;
            }

            return Response;
        }

        public void Dispose()
        {
            Response?.Dispose();
        }

        public ResponseStatus? Status => Exception?.Status ?? Response?.Status.KnownStatus;
    }

    #endregion
}
