using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ClientCertificate;

public sealed class ClientCertificateAuthenticationBuilder : IConcernBuilder
{
    private readonly ClientCertificateAuthenticationOptions _options = new();

    /// <summary>
    /// Sets a delegate that will be invoked to check whether the client
    /// is allowed to access the inner content. If the delegate returns false,
    /// the client will receive a 403 forbidden response.
    /// </summary>
    /// <param name="authorizer">The delegate to be invoked for authorization</param>
    public ClientCertificateAuthenticationBuilder Authorization(Func<IRequest, X509Certificate?, ValueTask<bool>> authorizer)
    {
        _options.Authorizer = authorizer;
        return this;
    }

    /// <summary>
    /// Sets a delegate that will be invoked to set the user record on the request, allowing
    /// inner handlers to consume this information as needed.
    /// </summary>
    /// <param name="userMapping">The delegate to determine the user</param>
    public ClientCertificateAuthenticationBuilder UserMapping(Func<IRequest, X509Certificate?, ValueTask<IUser?>> userMapping)
    {
        _options.UserMapping = userMapping;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        return new ClientCertificateAuthenticationConcern(content, _options);
    }

}
