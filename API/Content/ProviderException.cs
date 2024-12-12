using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content;

/// <summary>
/// If thrown by a content provider or router, the server will return
/// the specified HTTP response status to the client instead of
/// indicating a server error.
/// </summary>
[Serializable]
public class ProviderException : Exception
{

    #region Get-/Setters

    /// <summary>
    /// The status to be returned to the client.
    /// </summary>
    public ResponseStatus Status { get; }

    /// <summary>
    /// Modifications to be applied to the generated HTTP response.
    /// </summary>
    public Action<IResponseBuilder>? Modifications { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Raises an exception that allows the server to derive a HTTP response status from.
    /// </summary>
    /// <param name="status">The status of the HTTP response to be set</param>
    /// <param name="message">The error message to return to the client</param>
    /// <param name="modifications">The modifications to be applied to the generated response</param>
    public ProviderException(ResponseStatus status, string message, Action<IResponseBuilder>? modifications = null) : base(message)
    {
        Status = status;
        Modifications = modifications;
    }

    /// <summary>
    /// Raises an exception that allows the server to derive a HTTP response status from.
    /// </summary>
    /// <param name="status">The status of the HTTP response to be set</param>
    /// <param name="message">The error message to return to the client</param>
    /// <param name="inner">The original exception that caused this error</param>
    /// <param name="modifications">The modifications to be applied to the generated response</param>
    public ProviderException(ResponseStatus status, string message, Exception inner, Action<IResponseBuilder>? modifications = null) : base(message, inner)
    {
        Status = status;
        Modifications = modifications;
    }

    #endregion

}
