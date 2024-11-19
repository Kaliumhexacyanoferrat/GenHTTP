using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Client;

internal sealed class ClientAuthenticationOptions
{

    internal Func<IRequest, X509Certificate?, ValueTask<bool>>? Authenticator { get; set; }

    internal Func<IRequest, X509Certificate?, ValueTask<IUser?>>? UserMapping { get; set; }

}
