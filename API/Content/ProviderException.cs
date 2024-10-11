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

    #endregion

    #region Initialization

    public ProviderException(ResponseStatus status, string message) : base(message)
    {
            Status = status;
        }

    public ProviderException(ResponseStatus status, string message, Exception inner) : base(message, inner)
    {
            Status = status;
        }

    #endregion

}
