using System.Reflection;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Embedded;

public sealed class EmbeddedResourceBuilder : IResourceBuilder<EmbeddedResourceBuilder>
{
    private Assembly? _assembly;

    private DateTime? _modified;

    private string? _path, _name;

    private ContentType? _type;

    #region Functionality

    public EmbeddedResourceBuilder Assembly(Assembly assembly)
    {
        _assembly = assembly;
        return this;
    }

    public EmbeddedResourceBuilder Path(string name)
    {
        _path = name;
        return this;
    }

    public EmbeddedResourceBuilder Name(string name)
    {
        _name = name;
        return this;
    }

    public EmbeddedResourceBuilder Type(ContentType contentType)
    {
        _type = contentType;
        return this;
    }

    public EmbeddedResourceBuilder Modified(DateTime modified)
    {
        _modified = modified;
        return this;
    }

    public IResource Build()
    {
        var path = _path ?? throw new BuilderMissingPropertyException("path");

        var assembly = _assembly ?? System.Reflection.Assembly.GetCallingAssembly();

        var modified = _modified ?? assembly.GetModificationDate();

        var type = _type ?? path.GuessContentType() ?? ContentType.ApplicationForceDownload;

        return new EmbeddedResource(assembly, path, _name, type, modified);
    }

    #endregion

}
