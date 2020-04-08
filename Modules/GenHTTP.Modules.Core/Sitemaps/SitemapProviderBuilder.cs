using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapProviderBuilder : IHandlerBuilder
    {

        #region Functionality

        public IHandler Build(IHandler parent)
        {
            return new SitemapProvider(parent);
        }

        #endregion

    }

}
