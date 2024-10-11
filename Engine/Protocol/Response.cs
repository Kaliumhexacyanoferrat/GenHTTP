using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol;

internal sealed class Response : IResponse
{
    private static readonly FlexibleResponseStatus StatusOk = new(ResponseStatus.Ok);

    private readonly ResponseHeaderCollection _Headers = new();

    private CookieCollection? _Cookies;

    #region Initialization

    internal Response()
    {
        Status = StatusOk;
    }

    #endregion

    #region Functionality

    public void SetCookie(Cookie cookie)
    {
        WriteableCookies[cookie.Name] = cookie;
    }

    #endregion

    #region Get-/Setters

    public FlexibleResponseStatus Status { get; set; }

    public DateTime? Expires { get; set; }

    public DateTime? Modified { get; set; }

    public FlexibleContentType? ContentType { get; set; }

    public string? ContentEncoding { get; set; }

    public ulong? ContentLength { get; set; }

    public IResponseContent? Content { get; set; }

    public ICookieCollection Cookies => WriteableCookies;

    public bool HasCookies => _Cookies is not null && _Cookies.Count > 0;

    public IEditableHeaderCollection Headers => _Headers;

    public string? this[string field]
    {
        get => _Headers.GetValueOrDefault(field);
        set
        {
            if (value is not null)
            {
                _Headers[field] = value;
            }
            else _Headers.Remove(field);
        }
    }

    internal CookieCollection WriteableCookies
    {
        get { return _Cookies ??= new CookieCollection(); }
    }

    #endregion

    #region IDisposable Support

    private bool _Disposed;

    public void Dispose()
    {
        if (!_Disposed)
        {
            Headers.Dispose();

            _Cookies?.Dispose();

            if (Content is IDisposable disposableContent)
            {
                disposableContent.Dispose();
            }

            _Disposed = true;
        }
    }

    #endregion

}
