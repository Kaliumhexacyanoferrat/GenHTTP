using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings;

public sealed class StringResourceBuilder : IResourceBuilder<StringResourceBuilder>
{
    private string? _content, _name;

    private FlexibleContentType? _contentType;

    private DateTime? _modified;

    #region Functionality

    public StringResourceBuilder Content(string content)
    {
        _content = content;
        return this;
    }

    public StringResourceBuilder Name(string name)
    {
        _name = name;
        return this;
    }

    public StringResourceBuilder Type(FlexibleContentType contentType)
    {
        _contentType = contentType;
        return this;
    }

    public StringResourceBuilder Modified(DateTime modified)
    {
        _modified = modified;
        return this;
    }

    public IResource Build()
    {
        var content = _content ?? throw new BuilderMissingPropertyException("content");

        return new StringResource(content, _name, _contentType, _modified);
    }

    #endregion

}
