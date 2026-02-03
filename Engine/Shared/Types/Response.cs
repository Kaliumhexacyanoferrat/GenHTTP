using GenHTTP.Api.Protocol;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Engine.Shared.Types;

public sealed class Response : IResponse
{
    private static readonly FlexibleResponseStatus StatusOk = new(ResponseStatus.Ok);

    private readonly ResponseHeaderCollection _headers = new();

    private readonly CookieCollection _cookies = new();

    #region Initialization

    public Response()
    {
        Status = StatusOk;
        Connection = Api.Protocol.Connection.KeepAlive;
    }

    #endregion

    #region Get-/Setters

    public FlexibleResponseStatus Status { get; set; }

    public Connection Connection { get; set; }

    public DateTime? Expires { get; set; }

    public DateTime? Modified { get; set; }

    public FlexibleContentType? ContentType { get; set; }

    public string? ContentEncoding { get; set; }

    public ulong? ContentLength { get; set; }

    public IResponseContent? Content { get; set; }

    public ICookieCollection Cookies => WriteableCookies;

    public bool HasCookies => _cookies.Count > 0;

    public IEditableHeaderCollection Headers => _headers;

    public string? this[string field]
    {
        get => _headers.GetValueOrDefault(field);
        set
        {
            if (value is not null)
            {
                _headers[field] = value;
            }
            else
            {
                _headers.Remove(field);
            }
        }
    }

    internal CookieCollection WriteableCookies => _cookies;

    #endregion

    #region Functionality

    public void SetCookie(Cookie cookie)
    {
        WriteableCookies[cookie.Name] = cookie;
    }

    public void Reset()
    {
        Status = StatusOk;
        Connection = Connection.KeepAlive;

        _headers.Clear();
        _cookies.Clear();

        Expires = null;
        Modified = null;

        ReleaseContent();

        ContentType = null;
        ContentLength = null;
        ContentEncoding = null;
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            ReleaseContent();

            _disposed = true;
        }
    }

    private void ReleaseContent()
    {
        if (Content != null)
        {
            if (Content is IDisposable disposableContent)
            {
                disposableContent.Dispose();
            }

            Content = null;
        }
    }

    #endregion

}
