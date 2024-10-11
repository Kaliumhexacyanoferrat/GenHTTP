namespace GenHTTP.Api.Protocol;

/// <summary>
/// Represents a cookie that can be send to or received from a client.
/// </summary>
public struct Cookie
{

    #region Get-/Setters

    /// <summary>
    /// The name of the cookie.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The value of the cookie.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The number of seconds after the cookie will be discarded by the client.
    /// </summary>
    public ulong? MaxAge { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new cookie with the given name and value.
    /// </summary>
    /// <param name="name">The name of the cookie</param>
    /// <param name="value">The value of the cookie</param>
    public Cookie(string name, string value)
    {
        Name = name;
        Value = value;

        MaxAge = null;
    }

    /// <summary>
    /// Creates a new cookie with the given name and value.
    /// </summary>
    /// <param name="name">The name of the cookie</param>
    /// <param name="value">The value of the cookie</param>
    /// <param name="maxAge">The number of seconds until the cookie will be discarded</param>
    public Cookie(string name, string value, ulong maxAge) : this(name, value)
    {
        MaxAge = maxAge;
    }

    #endregion

}
