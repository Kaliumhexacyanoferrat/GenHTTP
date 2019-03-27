using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderRender<T> : IRenderer<T> where T : IBaseModel
    {

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }

        #endregion

        #region Initialization

        public PlaceholderRender(IResourceProvider templateProvider)
        {
            TemplateProvider = templateProvider;
        }

        #endregion

        #region Functionality

        public string Render(T model)
        {
            var template = TemplateProvider.GetResourceAsString();

            // ToDo replace und so

            return template;
        }

        #endregion

    }

}
