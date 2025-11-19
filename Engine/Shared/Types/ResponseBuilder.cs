using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class ResponseBuilder(Response response) : IResponseBuilder
{
    
    #region Functionality

    public IResponseBuilder Length(ulong length)
    {
        response.ContentLength = length;
        return this;
    }

    public IResponseBuilder Content(IResponseContent content)
    {
        response.Content = content;
        response.ContentLength = content.Length;

        return this;
    }

    public IResponseBuilder Type(FlexibleContentType contentType)
    {
        response.ContentType = contentType;
        return this;
    }

    public IResponseBuilder Cookie(Cookie cookie)
    {
        response.WriteableCookies[cookie.Name] = cookie;
        return this;
    }

    public IResponseBuilder Header(string key, string value)
    {
        response.Headers.Add(key, value);
        return this;
    }

    public IResponseBuilder Encoding(string encoding)
    {
        response.ContentEncoding = encoding;
        return this;
    }

    public IResponseBuilder Expires(DateTime expiryDate)
    {
        response.Expires = expiryDate;
        return this;
    }

    public IResponseBuilder Modified(DateTime modificationDate)
    {
        response.Modified = modificationDate;
        return this;
    }

    public IResponseBuilder Status(ResponseStatus status)
    {
        response.Status = new FlexibleResponseStatus(status);
        return this;
    }

    public IResponseBuilder Status(int status, string reason)
    {
        response.Status = new FlexibleResponseStatus(status, reason);
        return this;
    }
    
    public void Reset()
    {
        response.Reset();
    }

    public IResponse Build() => response;

    #endregion

}
