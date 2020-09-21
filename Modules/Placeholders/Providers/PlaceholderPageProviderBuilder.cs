using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PlaceholderPageProviderBuilder<T> : IHandlerBuilder<PlaceholderPageProviderBuilder<T>> where T : PageModel
    {
        private IResourceProvider? _TemplateProvider;
        private ModelProvider<T>? _ModelProvider;
        private string? _Title;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

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

        public PlaceholderPageProviderBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
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

            return Concerns.Chain(parent, _Concerns, (p) => new PlaceholderPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Title));
        }

        #endregion

    }

}
