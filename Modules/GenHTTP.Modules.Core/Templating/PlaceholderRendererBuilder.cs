using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderRendererBuilder : IBuilder<PlaceholderRender<TemplateModel>>
    {
        protected IResourceProvider? _TemplateProvider;
        
        #region Functionality

        public PlaceholderRendererBuilder TemplateProvider(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public PlaceholderRender<TemplateModel> Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new PlaceholderRender<TemplateModel>(_TemplateProvider);
        }

        #endregion

    }

}
