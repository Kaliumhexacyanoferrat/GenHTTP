using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PlaceholderRender<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly static Regex PLACEHOLDER = new Regex(@"\[([a-zA-Z0-9\.]+)\]", RegexOptions.Compiled);

        private string? _Template;

        #region Get-/Setters

        public CachedResource TemplateProvider { get; }

        #endregion

        #region Initialization

        public PlaceholderRender(IResource templateProvider)
        {
            TemplateProvider = new CachedResource(templateProvider);
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
            if (TemplateProvider.Changed)
            {
                _Template = TemplateProvider.GetResourceAsString();
            }

            return _Template!;
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
                    return data?.ToString();
                }
                else
                {
                    if (data == null)
                    {
                        return null;
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
