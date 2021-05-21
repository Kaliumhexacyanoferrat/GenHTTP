using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PlaceholderPageProviderBuilder<T> : IHandlerBuilder<PlaceholderPageProviderBuilder<T>>, IContentInfoBuilder<PlaceholderPageProviderBuilder<T>> where T : class, IModel
    {
        private IResource? _TemplateProvider;

        private ModelProvider<T>? _ModelProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        #region Functionality

        public PlaceholderPageProviderBuilder<T> Template(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Model(Func<IRequest, IHandler, T> modelProvider)
        {
            _ModelProvider = (r, h) => new ValueTask<T>(modelProvider(r, h));
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Add(IConcernBuilder concern)
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

            return Concerns.Chain(parent, _Concerns, (p) => new PlaceholderPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Info.Build()));
        }

        #endregion

    }

}
