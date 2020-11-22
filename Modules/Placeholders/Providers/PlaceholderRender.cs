using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;

using PooledAwait;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PlaceholderRender<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly static Regex PLACEHOLDER = new(@"\[([a-zA-Z0-9\.]+)\]", RegexOptions.Compiled);

        private string? _Template;

        #region Get-/Setters

        public ChangeTrackingResource TemplateProvider { get; }

        #endregion

        #region Initialization

        public PlaceholderRender(IResource templateProvider)
        {
            TemplateProvider = new(templateProvider);
        }

        #endregion

        #region Functionality

        public async ValueTask<string> RenderAsync(T model)
        {
            var template = await GetTemplate();

            return PLACEHOLDER.Replace(template, (match) =>
            {
                var fullPath = match.Groups[1].Value;
                var path = fullPath.Split('.');

                return GetValue(fullPath, path, model) ?? string.Empty;
            });
        }

        private async PooledValueTask<string> GetTemplate()
        {
            if (await TemplateProvider.HasChanged())
            {
                _Template = await TemplateProvider.GetResourceAsStringAsync();
            }

            return _Template!;
        }

        private static string? GetValue(string fullPath, IEnumerable<string> path, object model)
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
                    if (data is null)
                    {
                        return null;
                    }

                    return GetValue(fullPath, path.Skip(1), data);
                }
            }
        }

        private static object? GetValue(string name, object model)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var property = model.GetType().GetProperty(name, flags);

            if (property is not null)
            {
                return property.GetValue(model);
            }

            return null;
        }

        #endregion

    }

}
