using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderRender<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly static Regex PLACEHOLDER = new Regex(@"\[([a-zA-Z0-9\.]+)\]", RegexOptions.Compiled);

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

            return PLACEHOLDER.Replace(template, (match) =>
            {
                var fullPath = match.Groups[1].Value;
                var path = fullPath.Split('.');

                return GetValue(fullPath, path, model);
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

        private string? GetValue(string fullPath, IEnumerable<string> path, object model)
        {
            if (!path.Any())
            {
                return null;
            }
            else
            {
                var name = path.First();

                var data = GetValue(name, model);

                if (path.Count() == 1)
                {
                    return data?.ToString() ?? fullPath;
                }
                else
                {
                    if (data == null)
                    {
                        return fullPath;
                    }

                    return GetValue(fullPath, path.Skip(1), data);
                }
            }
        }

        private object? GetValue(string name, object model)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var property = model.GetType().GetProperty(name, flags);

            if (property != null)
            {
                return property.GetValue(model);
            }

            return null;
        }

        #endregion

    }

}
