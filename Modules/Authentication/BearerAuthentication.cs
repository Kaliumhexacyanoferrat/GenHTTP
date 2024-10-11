using GenHTTP.Modules.Authentication.Bearer;

namespace GenHTTP.Modules.Authentication;

public static class BearerAuthentication
{

    /// <summary>
    /// Creates a concern that will read an access token from
    /// the authorization headers and validate it according to
    /// its configuration.
    /// </summary>
    /// <returns>The newly created concern</returns>
    public static BearerAuthenticationConcernBuilder Create() => new();
}
