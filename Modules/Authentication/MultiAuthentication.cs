using GenHTTP.Api.Content;
using GenHTTP.Modules.Authentication.Multi;

namespace GenHTTP.Modules.Authentication;

public static class MultiAuthentication
{
    #region Builder

    /// <summary>
    /// Creates a authentication handler that will use
    /// underlying handlers to authenticate the request.
    /// </summary>
    public static MultiConcernBuilder Create() => new();

    #endregion

    #region Extensions

    /// <summary>
    /// Adds multi authentication to the handler.
    /// </summary>
    /// <param name="apiKeyAuth">The authentication concern to be added</param>
    public static T Add<T>(this T builder, MultiConcernBuilder apiKeyAuth) where T : IHandlerBuilder<T>
    {
        builder.Add(apiKeyAuth);
        return builder;
    }

    #endregion
}
