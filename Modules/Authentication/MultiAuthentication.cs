using GenHTTP.Modules.Authentication.Multi;

namespace GenHTTP.Modules.Authentication;

public static class MultiAuthentication
{
    #region Builder

    /// <summary>
    /// Creates a authentication handler that will use
    /// underlying handlers to authenticate the request.
    /// </summary>
    public static MultiAuthenticationConcernBuilder Create() => new();

    #endregion
}
