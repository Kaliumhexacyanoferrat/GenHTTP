using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Client;

public sealed class ClientAuthenticationConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private ClientAuthenticationOptions Options { get; }

    #endregion

    #region Initialization

    internal ClientAuthenticationConcern(IHandler content, ClientAuthenticationOptions options)
    {
        Content = content;
        Options = options;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var certificate = request.Client.Certificate;

        if (Options.Authenticator != null)
        {
            if (!await Options.Authenticator(request, certificate))
            {
                throw new ProviderException(ResponseStatus.Forbidden, "You are not allowed to access this resource");
            }
        }

        if (Options.UserMapping != null)
        {
            var user = await Options.UserMapping.Invoke(request, certificate);

            if (user != null)
            {
                request.SetUser(user);
            }
        }

        return await Content.HandleAsync(request);
    }

    #endregion

}
