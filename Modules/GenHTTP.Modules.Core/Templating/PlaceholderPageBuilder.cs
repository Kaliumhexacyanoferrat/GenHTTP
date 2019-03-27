using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderPageBuilder<T> : IBuilder<PlaceholderPageProvider<T>> where T : PageModel
    {
        protected IResourceProvider? _TemplateProvider;
        protected IPageProvider<T>? _ModelProvider;

        #region Functionality

        public PlaceholderPageBuilder<T> Template(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public PlaceholderPageBuilder<T> Model(IPageProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public PlaceholderPageProvider<T> Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider == null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return new PlaceholderPageProvider<T>(_TemplateProvider, _ModelProvider);
        }

        #endregion

    }

}
