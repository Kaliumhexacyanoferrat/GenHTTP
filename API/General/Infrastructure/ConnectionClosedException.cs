namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Will be thrown when writing a response to a client that has
/// already closed the connection.
/// </summary>
/// <remarks>
/// Derives from <see cref="IOException"/>, as this is what a stream throws
/// if its connection is gone and what content written to one already handles.
/// It has a type of its own so that engines can tell a client that went away -
/// which is nothing to report - from content that failed.
/// </remarks>
[Serializable]
public sealed class ConnectionClosedException : IOException
{

    #region Initialization

    public ConnectionClosedException() : base("The client closed the connection")
    {

    }

    #endregion

}
