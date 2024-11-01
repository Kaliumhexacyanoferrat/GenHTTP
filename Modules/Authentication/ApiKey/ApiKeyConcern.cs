using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ApiKey;

public sealed class ApiKeyConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private Func<IRequest, string?> KeyExtractor { get; }

    private Func<IRequest, string, ValueTask<IUser?>> Authenticator { get; }

    #endregion

    #region Initialization

    public ApiKeyConcern(IHandler content, Func<IRequest, string?> keyExtractor, Func<IRequest, string, ValueTask<IUser?>> authenticator)
    {
        Content = content;

        KeyExtractor = keyExtractor;
        Authenticator = authenticator;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var key = KeyExtractor(request);

        if (key != null)
        {
            var user = await Authenticator(request, key);

            if (user != null)
            {
                request.SetUser(user);

                return await Content.HandleAsync(request);
            }
            return request.Respond()
                          .Status(ResponseStatus.Forbidden)
                          .Build();
        }

        return request.Respond()
                      .Status(ResponseStatus.Unauthorized)
                      .Build();
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
