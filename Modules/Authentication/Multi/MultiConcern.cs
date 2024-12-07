using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Multi;

public sealed class MultiConcern : IConcern
{
    #region Get-/Setters

    public IHandler Content { get; }

    private readonly IConcern[] _delegatingConcerns;

    #endregion

    #region Initialization

    public MultiConcern(IHandler content, IConcern[] delegatingConcerns)
    {
        Content = content;
        _delegatingConcerns = delegatingConcerns ?? [];
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        IResponse? lastResponse = null;
        foreach (var concern in _delegatingConcerns)
        {
            lastResponse = await concern.HandleAsync(request);

            if (lastResponse?.Status.KnownStatus != ResponseStatus.Unauthorized)
            {
                return lastResponse;
            }
        }

        return lastResponse ??
               request.Respond()
                      .Status(ResponseStatus.Unauthorized)
                      .Build();
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
