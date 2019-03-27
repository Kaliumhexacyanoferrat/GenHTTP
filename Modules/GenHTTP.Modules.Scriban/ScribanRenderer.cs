using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

using Scriban;
using Scriban.Runtime;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanRenderer<T> : IRenderer<T> where T : IBaseModel
    {

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }
        
        #endregion

        #region Initialization

        public ScribanRenderer(IResourceProvider templateProvider)
        {
            TemplateProvider = templateProvider;
        }

        #endregion

        #region Functionality

        public string Render(T model)
        {
            var content = TemplateProvider.GetResourceAsString();

            var template = Template.Parse(content);

            var obj = new ScriptObject();

            obj.Import(model);
            obj.SetValue("route", new RoutingMethod(model.Request.Routing), true);

            return template.Render(obj);
        }

        #endregion

    }

}
