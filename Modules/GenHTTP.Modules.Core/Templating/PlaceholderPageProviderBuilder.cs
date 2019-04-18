using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderPageProviderBuilder<T> : ContentBuilderBase where T : PageModel
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

        public override IContentProvider Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider == null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return new PlaceholderPageProvider<T>(_TemplateProvider, _ModelProvider, _Title, _Modification);
        }

        #endregion

    }

}
