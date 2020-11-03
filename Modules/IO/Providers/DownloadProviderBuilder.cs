using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers
{

    public class DownloadProviderBuilder : IHandlerBuilder<DownloadProviderBuilder>
    {
        private IResource? _ResourceProvider;

        private string? _FileName;

        private FlexibleContentType? _ContentType;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public DownloadProviderBuilder Resource(IResource resource)
        {
            _ResourceProvider = resource;
            return this;
        }

        public DownloadProviderBuilder Type(ContentType contentType) => Type(new FlexibleContentType(contentType));

        public DownloadProviderBuilder Type(FlexibleContentType contentType)
        {
            _ContentType = contentType;
            return this;
        }

        public DownloadProviderBuilder FileName(string fileName)
        {
            _FileName = fileName;
            return this;
        }

        public DownloadProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var resource = _ResourceProvider ?? throw new BuilderMissingPropertyException("resourceProvider");

            return Concerns.Chain(parent, _Concerns, (p) => new DownloadProvider(p, resource, _FileName, _ContentType));
        }

        #endregion

    }

}
