using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ClientCertificate;

internal sealed class ClientCertificateAuthenticationOptions
{

    internal Func<IRequest, X509Certificate?, ValueTask<bool>>? Authorizer { get; set; }

    internal Func<IRequest, X509Certificate?, ValueTask<IUser?>>? UserMapping { get; set; }

}
