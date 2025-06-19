namespace GenHTTP.Api.Protocol;

/// <summary>
/// Thrown by the server, if the HTTP protocol has
/// somehow been violated (either by the server or the client).
/// </summary>
[Serializable]
public class ProtocolException : Exception
{

    public ProtocolException(string reason) : base(reason)
    {

    }

    [Obsolete("This constructor is not required by the engine and will be removed in GenHTTP 10")]
    public ProtocolException(string reason, Exception inner) : base(reason, inner)
    {

    }

}
