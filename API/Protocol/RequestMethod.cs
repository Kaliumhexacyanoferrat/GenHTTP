namespace GenHTTP.Api.Protocol;

public enum RequestMethod
{
    Get,
    Head,
    Post,
    Put,
    Patch,
    Delete,
    Options,
    PropFind,
    PropPatch,
    MkCol,
    Copy,
    Move,
    Lock,
    Unlock
}

/// <summary>
/// The kind of request sent by the client.
/// </summary>
public class FlexibleRequestMethod
{
    private static readonly Dictionary<string, FlexibleRequestMethod> RawCache = new(StringComparer.InvariantCultureIgnoreCase)
    {
        {
            "HEAD", new FlexibleRequestMethod(RequestMethod.Head)
        },
        {
            "GET", new FlexibleRequestMethod(RequestMethod.Get)
        },
        {
            "POST", new FlexibleRequestMethod(RequestMethod.Post)
        },
        {
            "PUT", new FlexibleRequestMethod(RequestMethod.Put)
        },
        {
            "DELETE", new FlexibleRequestMethod(RequestMethod.Delete)
        },
        {
            "OPTIONS", new FlexibleRequestMethod(RequestMethod.Options)
        }
    };

    private static readonly Dictionary<RequestMethod, FlexibleRequestMethod> KnownCache = new()
    {
        {
            RequestMethod.Head, new FlexibleRequestMethod(RequestMethod.Head)
        },
        {
            RequestMethod.Get, new FlexibleRequestMethod(RequestMethod.Get)
        },
        {
            RequestMethod.Post, new FlexibleRequestMethod(RequestMethod.Post)
        },
        {
            RequestMethod.Put, new FlexibleRequestMethod(RequestMethod.Put)
        },
        {
            RequestMethod.Delete, new FlexibleRequestMethod(RequestMethod.Delete)
        },
        {
            RequestMethod.Options, new FlexibleRequestMethod(RequestMethod.Options)
        }
    };

    #region Mapping

    private static readonly Dictionary<string, RequestMethod> Mapping = new(StringComparer.OrdinalIgnoreCase)
    {
        {
            "GET", RequestMethod.Get
        },
        {
            "HEAD", RequestMethod.Head
        },
        {
            "POST", RequestMethod.Post
        },
        {
            "PUT", RequestMethod.Put
        },
        {
            "PATCH", RequestMethod.Patch
        },
        {
            "DELETE", RequestMethod.Delete
        },
        {
            "OPTIONS", RequestMethod.Options
        },
        {
            "PROPFIND", RequestMethod.PropFind
        },
        {
            "PROPPATCH", RequestMethod.PropPatch
        },
        {
            "MKCOL", RequestMethod.MkCol
        },
        {
            "COPY", RequestMethod.Copy
        },
        {
            "MOVE", RequestMethod.Move
        },
        {
            "LOCK", RequestMethod.Lock
        },
        {
            "UNLOCK", RequestMethod.Unlock
        }
    };

    #endregion

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

        if (Mapping.TryGetValue(rawType, out var type))
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
        if (RawCache.TryGetValue(rawMethod, out var found))
        {
            return found;
        }

        var method = new FlexibleRequestMethod(rawMethod);

        RawCache[rawMethod] = method;

        return method;
    }

    /// <summary>
    /// Fetches a cached instance for the given content type.
    /// </summary>
    /// <param name="knownMethod">The known value to be resolved</param>
    /// <returns>The content type instance to be used</returns>
    public static FlexibleRequestMethod Get(RequestMethod knownMethod)
    {
        if (KnownCache.TryGetValue(knownMethod, out var found))
        {
            return found;
        }

        var method = new FlexibleRequestMethod(knownMethod);

        KnownCache[knownMethod] = method;

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
