using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

/// <summary>
///     Allows to build a response modification object so that
///     individual handlers do not need to implement the logic
///     theirselves.
/// </summary>
public class ResponseModificationBuilder : IResponseModification<ResponseModificationBuilder>, IBuilder<ResponseModifications?>
{

    private FlexibleContentType? _ContentType;

    private List<Cookie>? _Cookies;

    private string? _Encoding;

    private DateTime? _ExpiryDate, _ModificationDate;

    private Dictionary<string, string>? _Headers;
    private FlexibleResponseStatus? _Status;

    #region Functionality

    public ResponseModificationBuilder Cookie(Cookie cookie)
    {
        if (_Cookies == null)
        {
            _Cookies = new List<Cookie>();
        }

        _Cookies.Add(cookie);

        return this;
    }

    public ResponseModificationBuilder Encoding(string encoding)
    {
        _Encoding = encoding;
        return this;
    }

    public ResponseModificationBuilder Expires(DateTime expiryDate)
    {
        _ExpiryDate = expiryDate;
        return this;
    }

    public ResponseModificationBuilder Header(string key, string value)
    {
        if (_Headers == null)
        {
            _Headers = new Dictionary<string, string>();
        }

        _Headers[key] = value;

        return this;
    }

    public ResponseModificationBuilder Modified(DateTime modificationDate)
    {
        _ModificationDate = modificationDate;
        return this;
    }

    public ResponseModificationBuilder Status(ResponseStatus status)
    {
        _Status = new FlexibleResponseStatus(status);
        return this;
    }

    public ResponseModificationBuilder Status(int status, string reason)
    {
        _Status = new FlexibleResponseStatus(status, reason);
        return this;
    }

    public ResponseModificationBuilder Type(FlexibleContentType contentType)
    {
        _ContentType = contentType;
        return this;
    }

    public ResponseModifications? Build()
    {
        if (_Status != null || _Encoding != null || null != _ContentType
            || _ExpiryDate != null || _ModificationDate != null
            || _Cookies?.Count > 0 || _Headers?.Count > 0)
        {
            return new ResponseModifications(_Status, _ContentType, _Cookies, _Encoding, _ExpiryDate, _ModificationDate, _Headers);
        }

        return null;
    }

    #endregion

}
