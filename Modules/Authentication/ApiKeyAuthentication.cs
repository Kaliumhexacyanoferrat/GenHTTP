using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.ApiKey;

namespace GenHTTP.Modules.Authentication;

/// <summary>
/// Allows API key authentication to be added to a handler instance
/// as a concern.
/// </summary>
public static class ApiKeyAuthentication
{

    #region Builder

    /// <summary>
    /// Creates a customizable API key authentication handler that will
    /// read the key from the HTTP header named "X-API-Key".
    /// </summary>
    public static ApiKeyConcernBuilder Create() => new();

    #endregion

    #region Extensions

    /// <summary>
    /// Adds API key authentication to the handler.
    /// </summary>
    /// <param name="apiKeyAuth">The authentication concern to be added</param>
    public static T Authentication<T>(this T builder, ApiKeyConcernBuilder apiKeyAuth) where T : IHandlerBuilder<T>
    {
            builder.Add(apiKeyAuth);
            return builder;
        }

    /// <summary>
    /// Adds API key authentication to the handler, using the
    /// given function to check, whether a key passed by the
    /// client is valid.
    /// </summary>
    /// <param name="authenticator">The function to be invoked to determine, whether the given string key is valid</param>
    public static T Authentication<T>(this T builder, Func<IRequest, string, ValueTask<IUser?>> authenticator) where T : IHandlerBuilder<T>
    {
            builder.Add(Create().Authenticator(authenticator));
            return builder;
        }

    #endregion

}
