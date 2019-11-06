using System.Reflection;
using System.Text.RegularExpressions;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderRender<T> : IRenderer<T> where T : IBaseModel
    {
        private readonly static Regex PLACEHOLDER = new Regex(@"\[([a-zA-Z0-9]+)\]");

        private string? _Template;

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
            var template = GetTemplate();

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            return PLACEHOLDER.Replace(template, (match) =>
            {
                var name = match.Groups[1].Value;

                var property = model.GetType().GetProperty(name, flags);

                if (property != null)
                {
                    return property.GetValue(model)?.ToString() ?? string.Empty;
                }

                return match.Value;
            });
        }

        private string GetTemplate()
        {
            if (TemplateProvider.AllowCache)
            {
                return _Template ?? (_Template = LoadTemplate());
            }

            return LoadTemplate();
        }

        private string LoadTemplate()
        {
            return TemplateProvider.GetResourceAsString();
        }

        #endregion

    }

}
