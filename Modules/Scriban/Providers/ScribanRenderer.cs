using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;

using Scriban;
using Scriban.Runtime;

namespace GenHTTP.Modules.Scriban.Providers
{

    public class ScribanRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private Template? _Template;

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
            var template = GetTemplate();

            var obj = new ScriptObject();

            obj.Import(model);
            obj.SetValue("route", new RoutingMethod(model), true);

            return template.Render(obj);
        }

        private Template GetTemplate()
        {
            if (TemplateProvider.AllowCache)
            {
                return _Template ?? (_Template = LoadTemplate());
            }

            return LoadTemplate();
        }

        private Template LoadTemplate()
        {
            return Template.Parse(TemplateProvider.GetResourceAsString());
        }

        #endregion

    }

}
