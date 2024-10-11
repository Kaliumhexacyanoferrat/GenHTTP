using System;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Embedded;

public sealed class EmbeddedResourceBuilder : IResourceBuilder<EmbeddedResourceBuilder>
{
    private Assembly? _Assembly;

    private string? _Path, _Name;

    private FlexibleContentType? _Type;

    private DateTime? _Modified;

    #region Functionality

    public EmbeddedResourceBuilder Assembly(Assembly assembly)
    {
            _Assembly = assembly;
            return this;
        }

    public EmbeddedResourceBuilder Path(string name)
    {
            _Path = name;
            return this;
        }

    public EmbeddedResourceBuilder Name(string name)
    {
            _Name = name;
            return this;
        }

    public EmbeddedResourceBuilder Type(FlexibleContentType contentType)
    {
            _Type = contentType;
            return this;
        }

    public EmbeddedResourceBuilder Modified(DateTime modified)
    {
            _Modified = modified;
            return this;
        }

    public IResource Build()
    {
            var path = _Path ?? throw new BuilderMissingPropertyException("path");

            var assembly = _Assembly ?? System.Reflection.Assembly.GetCallingAssembly();

            var modified = _Modified ?? assembly.GetModificationDate();

            var type = _Type ?? FlexibleContentType.Get(path.GuessContentType() ?? ContentType.ApplicationForceDownload);

            return new EmbeddedResource(assembly, path, _Name, type, modified);
        }

    #endregion

}
