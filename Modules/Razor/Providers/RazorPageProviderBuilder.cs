using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorPageProviderBuilder<T> : IHandlerBuilder<RazorPageProviderBuilder<T>>, IContentInfoBuilder<RazorPageProviderBuilder<T>> where T : PageModel
    {
        protected IResource? _TemplateProvider;

        protected ModelProvider<T>? _ModelProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        #region Functionality

        public RazorPageProviderBuilder<T> Template(IResource templateProvider)
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
            _Info.Title(title);
            return this;
        }

        public RazorPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public RazorPageProviderBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_TemplateProvider is null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider is null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new RazorPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Info.Build()));
        }

        #endregion

    }

}
