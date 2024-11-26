namespace GenHTTP.Testing;

/// <summary>
/// Specifies the engine the test host should
/// use to host a web server process.
/// </summary>
public enum TestEngine
{

    /// <summary>
    /// The built-in GenHTTP server engine.
    /// </summary>
    Internal,

    /// <summary>
    /// Microsoft Kestrel.
    /// </summary>
    Kestrel

}
