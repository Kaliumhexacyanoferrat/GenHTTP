using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.General
{

    public class PageProviderBuilder : ContentBuilderBase
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

        public override IContentProvider Build()
        {
            if (_Content == null)
            {
                throw new BuilderMissingPropertyException("Content");
            }

            return new PageProvider(_Title, _Content, _Modification);
        }

        #endregion

    }

}
