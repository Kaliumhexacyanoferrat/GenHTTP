using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ClientCertificate;

public sealed class ClientCertificateAuthenticationConcern : IDocumentedConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private ClientCertificateAuthenticationOptions Options { get; }

    private SecurityScheme? Scheme { get; }

    #endregion

    #region Initialization

    internal ClientCertificateAuthenticationConcern(IHandler content, ClientCertificateAuthenticationOptions options, SecurityScheme? scheme)
    {
        Content = content;
        Options = options;
        Scheme = scheme;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public void AddDocumentation(OperationDocumentation operation)
    {
        if (Scheme != null)
        {
            operation.RequireAuthentication(Scheme);
        }

        if (Options.Authorizer != null)
        {
            operation.Response(ResponseStatus.Forbidden, "The client certificate is not allowed to access this resource");
        }
    }

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var certificate = request.Client.Certificate;

        if (Options.Authorizer != null)
        {
            if (!await Options.Authorizer(request, certificate))
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
