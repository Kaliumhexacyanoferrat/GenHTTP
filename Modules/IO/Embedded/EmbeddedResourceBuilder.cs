using System;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using System.IO;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Embedded
{

    public class EmbeddedResourceBuilder : IResourceBuilder<EmbeddedResourceBuilder>
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

            var sourceFile = new FileInfo(assembly.CodeBase);

            var modified = _Modified ?? ((sourceFile.Exists) ? sourceFile.LastWriteTimeUtc : DateTime.UtcNow);

            var type = _Type ?? new FlexibleContentType(path.GuessContentType() ?? ContentType.ApplicationForceDownload);

            return new EmbeddedResource(assembly, path, _Name, type, modified);
        }

        #endregion

    }

}
