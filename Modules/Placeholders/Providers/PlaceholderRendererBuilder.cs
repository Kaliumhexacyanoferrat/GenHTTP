using System.Collections.Generic;
using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PlaceholderRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IModel
    {
        private IResource? _TemplateProvider;

        private readonly Dictionary<string, Func<IModel, object?, object?>> _CustomRenderers = new();

        #region Functionality

        public PlaceholderRendererBuilder<T> TemplateProvider(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public PlaceholderRendererBuilder<T> AddRenderer(string name, Func<IModel, object?, object> renderer)
        {
            _CustomRenderers.Add(name, renderer);
            return this;
        }

        public IRenderer<T> Build()
        {
            if (_TemplateProvider is null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new PlaceholderRender<T>(_TemplateProvider, _CustomRenderers);
        }

        #endregion

    }

}
