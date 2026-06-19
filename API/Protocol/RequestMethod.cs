namespace GenHTTP.Api.Protocol;

/// <summary>Represents an HTTP request method (verb).</summary>
[MemoryView]
public readonly partial struct RequestMethod
{

    #region Known Methods

    public static readonly RequestMethod Get = new("GET");

    public static readonly RequestMethod Head = new("HEAD");

    public static readonly RequestMethod Post = new("POST");

    public static readonly RequestMethod Put = new("PUT");

    public static readonly RequestMethod Delete = new("DELETE");

    public static readonly RequestMethod Connect = new("CONNECT");

    public static readonly RequestMethod Options = new("OPTIONS");

    public static readonly RequestMethod Trace = new("TRACE");

    public static readonly RequestMethod Patch = new("PATCH");

    #endregion

}
