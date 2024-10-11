using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Modules.Authentication.Basic;

namespace GenHTTP.Modules.Authentication;

/// <summary>
/// Allows basic authentication to be added to a handler instance
/// as a concern.
/// </summary>
public static class BasicAuthentication
{
    private const string DEFAULT_REALM = "Restricted Area";

    #region Builder

    /// <summary>
    /// Creates a basic authentication concern that will use the
    /// given lambda to check, whether an user is allowed to
    /// access the restricted area.
    /// </summary>
    /// <param name="authenticator">The lambda to be evaluated</param>
    /// <param name="realm">The name of the realm returned to the client</param>
    /// <returns>The newly created basic authentication concern</returns>
    public static BasicAuthenticationConcernBuilder Create(Func<string, string, ValueTask<IUser?>> authenticator, string realm = DEFAULT_REALM) => new BasicAuthenticationConcernBuilder().Handler(authenticator)
                                                                                                                                                                                          .Realm(realm);

    /// <summary>
    /// Creates a basic authentication concern that stores credentials in
    /// memory and can be used for quick development purposes.
    /// </summary>
    /// <param name="realm">The name of the realm returned to the client</param>
    /// <returns>The newly created basic authentication concern</returns>
    public static BasicAuthenticationKnownUsersBuilder Create(string realm = DEFAULT_REALM) => new BasicAuthenticationKnownUsersBuilder().Realm(realm);

    #endregion

    #region Extensions

    /// <summary>
    /// Adds basic authentication to the handler using the given lambda
    /// to check, whether users are allowed to access the restricted area.
    /// </summary>
    /// <param name="authenticator">The lambda to be evaluated on request</param>
    /// <param name="realm">The name of the realm to be returned to the client</param>
    public static T Authentication<T>(this T builder, Func<string, string, ValueTask<IUser?>> authenticator, string realm = DEFAULT_REALM) where T : IHandlerBuilder<T>
    {
        builder.Add(Create(authenticator, realm));
        return builder;
    }

    /// <summary>
    /// Adds basic authentication to the handler using the specified concern instance.
    /// </summary>
    /// <param name="basicAuth">The pre-configured concern instance to be used</param>
    public static T Authentication<T>(this T builder, BasicAuthenticationConcernBuilder basicAuth) where T : IHandlerBuilder<T>
    {
        builder.Add(basicAuth);
        return builder;
    }

    /// <summary>
    /// Adds basic authentication to the handler using the specified concern instance.
    /// </summary>
    /// <param name="basicAuth">The pre-configured concern instance to be used</param>
    public static T Authentication<T>(this T builder, BasicAuthenticationKnownUsersBuilder basicAuth) where T : IHandlerBuilder<T>
    {
        builder.Add(basicAuth);
        return builder;
    }

    #endregion

}
