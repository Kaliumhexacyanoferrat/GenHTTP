namespace GenHTTP.Api.Protocol;

/// <summary>Represents an HTTP request method (verb).</summary>
[MemoryView]
public readonly partial struct RequestMethod
{

    #region Known Methods

    public static readonly RequestMethod Get     = new("GET"u8.ToArray());
    
    public static readonly RequestMethod Head    = new("HEAD"u8.ToArray());
    
    public static readonly RequestMethod Post    = new("POST"u8.ToArray());
    
    public static readonly RequestMethod Put     = new("PUT"u8.ToArray());
    
    public static readonly RequestMethod Delete  = new("DELETE"u8.ToArray());
    
    public static readonly RequestMethod Connect = new("CONNECT"u8.ToArray());
    
    public static readonly RequestMethod Options = new("OPTIONS"u8.ToArray());
    
    public static readonly RequestMethod Trace   = new("TRACE"u8.ToArray());
    
    public static readonly RequestMethod Patch   = new("PATCH"u8.ToArray());

    #endregion

}
