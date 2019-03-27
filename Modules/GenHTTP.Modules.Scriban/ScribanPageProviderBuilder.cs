using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanPageProviderBuilder<T> : IBuilder<ScribanPageProvider<T>> where T : PageModel
    {
        protected IResourceProvider? _TemplateProvider;
        protected IPageProvider<T>? _ModelProvider;

        #region Functionality

        public ScribanPageProviderBuilder<T> Template(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public ScribanPageProviderBuilder<T> Model(IPageProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public ScribanPageProvider<T> Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider == null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return new ScribanPageProvider<T>(_TemplateProvider, _ModelProvider);
        }

        #endregion

    }

}
