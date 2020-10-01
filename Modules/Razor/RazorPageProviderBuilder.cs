using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Razor
{

    public class RazorPageProviderBuilder<T> : IHandlerBuilder where T : PageModel
    {
        protected IResourceProvider? _TemplateProvider;
        protected ModelProvider<T>? _ModelProvider;
        protected string? _Title;
        private string? _Description;

        #region Functionality

        public RazorPageProviderBuilder<T> Template(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public RazorPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public RazorPageProviderBuilder<T> Title(string title)
        {
            _Title = title;
            return this;
        }

        public RazorPageProviderBuilder<T> Description(string description)
        {
            _Description = description;
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

            return new RazorPageProvider<T>(parent, _TemplateProvider, _ModelProvider, _Title, _Description);
        }

        #endregion

    }

}
