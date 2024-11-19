using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Client;

public sealed class ClientAuthenticationBuilder : IConcernBuilder
{
    private readonly ClientAuthenticationOptions Options = new();

    public ClientAuthenticationBuilder Authentication(Func<IRequest, X509Certificate?, ValueTask<bool>> authenticator)
    {
        Options.Authenticator = authenticator;
        return this;
    }

    public ClientAuthenticationBuilder UserMapping(Func<IRequest, X509Certificate?, ValueTask<IUser?>> userMapping)
    {
        Options.UserMapping = userMapping;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        return new ClientAuthenticationConcern(content, Options);
    }

}
