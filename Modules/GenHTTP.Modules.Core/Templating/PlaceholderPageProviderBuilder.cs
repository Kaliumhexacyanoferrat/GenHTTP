using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderPageProviderBuilder<T> : IHandlerBuilder where T : PageModel
    {
        private IResourceProvider? _TemplateProvider;
        private ModelProvider<T>? _ModelProvider;
        private string? _Title;

        #region Functionality

        public PlaceholderPageProviderBuilder<T> Template(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Title(string title)
        {
            _Title = title;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider == null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return new PlaceholderPageProvider<T>(parent, _TemplateProvider, _ModelProvider, _Title);
        }

        #endregion

    }

}
