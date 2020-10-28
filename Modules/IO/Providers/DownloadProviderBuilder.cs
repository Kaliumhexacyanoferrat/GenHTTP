using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Providers
{

    public class DownloadProviderBuilder : IHandlerBuilder<DownloadProviderBuilder>
    {
        private IResource? _ResourceProvider;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public DownloadProviderBuilder Resource(IResource resource)
        {
            _ResourceProvider = resource;
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

            return Concerns.Chain(parent, _Concerns, (p) => new DownloadProvider(p, resource));
        }

        #endregion

    }

}
