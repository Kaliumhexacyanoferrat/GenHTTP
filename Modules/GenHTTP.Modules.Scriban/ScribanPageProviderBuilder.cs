using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanPageProviderBuilder<T> : IContentBuilder where T : PageModel
    {
        protected IResourceProvider? _TemplateProvider;
        protected ModelProvider<T>? _ModelProvider;
        protected string? _Title;

        #region Functionality

        public ScribanPageProviderBuilder<T> Template(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public ScribanPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public ScribanPageProviderBuilder<T> Title(string title)
        {
            _Title = title;
            return this;
        }

        public IContentProvider Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider == null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return new ScribanPageProvider<T>(_TemplateProvider, _ModelProvider, _Title);
        }

        #endregion

    }

}
