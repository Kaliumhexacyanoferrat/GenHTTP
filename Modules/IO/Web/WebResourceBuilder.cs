using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Web;

public class WebResourceBuilder : IResourceBuilder<WebResourceBuilder>
{
    private Uri? _source;

    private string? _fixedName;

    private FlexibleContentType? _fixedType;

    private DateTime? _fixedModificationDate;

    #region Functionality

    public WebResourceBuilder Source(Uri source)
    {
        _source = source;
        return this;
    }

    public WebResourceBuilder Source(string source)
    {
        if (Uri.TryCreate(source, UriKind.Absolute, out var parsed))
        {
            _source = parsed;
        }
        else
        {
            throw new ArgumentException("The given source URI is invalid or not absolute", nameof(source));
        }

        return this;
    }

    public WebResourceBuilder Name(string name)
    {
        _fixedName = name;
        return this;
    }

    public WebResourceBuilder Type(FlexibleContentType contentType)
    {
        _fixedType = contentType;
        return this;
    }

    public WebResourceBuilder Modified(DateTime modified)
    {
        _fixedModificationDate = modified;
        return this;
    }

    public IResource Build()
    {
        var source = _source ?? throw new BuilderMissingPropertyException("Source");

        if (!source.IsAbsoluteUri)
        {
            throw new ArgumentException("Only absolute URIs are supported", nameof(source));
        }

        if (source.Scheme != Uri.UriSchemeHttp && source.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Only HTTP/HTTPS sources are supported", nameof(source));
        }

        return new WebResource(source, _fixedName, _fixedModificationDate, _fixedType);
    }

    #endregion

}
