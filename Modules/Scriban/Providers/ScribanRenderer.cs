﻿using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;
using Scriban;
using Scriban.Runtime;

namespace GenHTTP.Modules.Scriban.Providers
{

    public class ScribanRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private Template? _Template;

        #region Get-/Setters

        public ChangeTrackingResource TemplateProvider { get; }

        #endregion

        #region Initialization

        public ScribanRenderer(IResource templateProvider)
        {
            TemplateProvider = new ChangeTrackingResource(templateProvider);
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
            if (TemplateProvider.Changed)
            {
                _Template = LoadTemplate();
            }

            return _Template!;
        }

        private Template LoadTemplate()
        {
            return Template.Parse(TemplateProvider.GetResourceAsString());
        }

        #endregion

    }

}
