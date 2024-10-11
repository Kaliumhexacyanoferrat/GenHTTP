namespace GenHTTP.Api.Protocol;

public enum RequestMethod
{
    GET,
    HEAD,
    POST,
    PUT,
    PATCH,
    DELETE,
    OPTIONS,
    PROPFIND,
    PROPPATCH,
    MKCOL,
    COPY,
    MOVE,
    LOCK,
    UNLOCK
}

/// <summary>
/// The kind of request sent by the client.
/// </summary>
public class FlexibleRequestMethod
{
    private static readonly Dictionary<string, FlexibleRequestMethod> _RawCache = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "HEAD", new(RequestMethod.HEAD) },
        { "GET", new(RequestMethod.GET) },
        { "POST", new(RequestMethod.POST) },
        { "PUT", new(RequestMethod.PUT) },
        { "DELETE", new(RequestMethod.DELETE) },
        { "OPTIONS", new(RequestMethod.OPTIONS) }
    };

    private static readonly Dictionary<RequestMethod, FlexibleRequestMethod> _KnownCache = new()
    {
        { RequestMethod.HEAD, new(RequestMethod.HEAD) },
        { RequestMethod.GET, new(RequestMethod.GET) },
        { RequestMethod.POST, new(RequestMethod.POST) },
        { RequestMethod.PUT, new(RequestMethod.PUT) },
        { RequestMethod.DELETE, new(RequestMethod.DELETE) },
        { RequestMethod.OPTIONS, new(RequestMethod.OPTIONS) }
    };

    #region Get-/Setters

    /// <summary>
    /// The known method of the request, if any.
    /// </summary>
    public RequestMethod? KnownMethod { get; }

    /// <summary>
    /// The raw method of the request.
    /// </summary>
    public string RawMethod { get; }

    #endregion

    #region Mapping

    private static readonly Dictionary<string, RequestMethod> MAPPING = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GET", RequestMethod.GET },
        { "HEAD", RequestMethod.HEAD },
        { "POST", RequestMethod.POST },
        { "PUT", RequestMethod.PUT },
        { "PATCH", RequestMethod.PATCH },
        { "DELETE", RequestMethod.DELETE },
        { "OPTIONS", RequestMethod.OPTIONS },
        { "PROPFIND", RequestMethod.PROPFIND },
        { "PROPPATCH", RequestMethod.PROPPATCH },
        { "MKCOL", RequestMethod.MKCOL },
        { "COPY", RequestMethod.COPY },
        { "MOVE", RequestMethod.MOVE },
        { "LOCK", RequestMethod.LOCK },
        { "UNLOCK", RequestMethod.UNLOCK }
    };

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new request method instance from a known type.
    /// </summary>
    /// <param name="method">The known type to be used</param>
    public FlexibleRequestMethod(RequestMethod method)
    {
            KnownMethod = method;
            RawMethod = Enum.GetName(method) ?? throw new ArgumentException("The given method cannot be mapped", nameof(method));
        }

    /// <summary>
    /// Create a new request method instance.
    /// </summary>
    /// <param name="rawType">The raw type transmitted by the client</param>
    public FlexibleRequestMethod(string rawType)
    {
            RawMethod = rawType;

            if (MAPPING.TryGetValue(rawType, out var type))
            {
                KnownMethod = type;
            }
            else
            {
                KnownMethod = null;
            }
        }

    #endregion

    #region Functionality

    /// <summary>
    /// Fetches a cached instance for the given content type.
    /// </summary>
    /// <param name="rawMethod">The raw string to be resolved</param>
    /// <returns>The content type instance to be used</returns>
    public static FlexibleRequestMethod Get(string rawMethod)
    {
            if (_RawCache.TryGetValue(rawMethod, out var found))
            {
                return found;
            }

            var method = new FlexibleRequestMethod(rawMethod);

            _RawCache[rawMethod] = method;

            return method;
        }

    /// <summary>
    /// Fetches a cached instance for the given content type.
    /// </summary>
    /// <param name="knownMethod">The known value to be resolved</param>
    /// <returns>The content type instance to be used</returns>
    public static FlexibleRequestMethod Get(RequestMethod knownMethod)
    {
            if (_KnownCache.TryGetValue(knownMethod, out var found))
            {
                return found;
            }

            var method = new FlexibleRequestMethod(knownMethod);

            _KnownCache[knownMethod] = method;

            return method;
        }

    #endregion

    #region Convenience

    public static bool operator ==(FlexibleRequestMethod method, RequestMethod knownMethod) => method.KnownMethod == knownMethod;

    public static bool operator !=(FlexibleRequestMethod method, RequestMethod knownMethod) => method.KnownMethod != knownMethod;

    public static bool operator ==(FlexibleRequestMethod method, string rawMethod) => method.RawMethod == rawMethod;

    public static bool operator !=(FlexibleRequestMethod method, string rawMethod) => method.RawMethod != rawMethod;

    public override bool Equals(object? obj) => obj is FlexibleRequestMethod method && RawMethod == method.RawMethod;

    public override int GetHashCode() => RawMethod.GetHashCode();

    #endregion

}
