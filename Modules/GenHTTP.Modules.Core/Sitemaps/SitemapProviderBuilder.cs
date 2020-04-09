using System.Collections.Generic;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapProviderBuilder : IHandlerBuilder<SitemapProviderBuilder>
    {

        private List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public SitemapProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new SitemapProvider(p));
        }

        #endregion

    }

}
