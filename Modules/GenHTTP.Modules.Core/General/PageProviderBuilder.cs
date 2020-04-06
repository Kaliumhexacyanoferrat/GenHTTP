using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class PageProviderBuilder : IHandlerBuilder
    {
        private IResourceProvider? _Content;
        private string? _Title;

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public PageProviderBuilder Content(IResourceProvider templateProvider)
        {
            _Content = templateProvider;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Content == null)
            {
                throw new BuilderMissingPropertyException("Content");
            }

            return new PageProvider(parent, _Title, _Content);
        }

        #endregion

    }

}
