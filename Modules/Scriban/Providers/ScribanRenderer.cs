using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;

using PooledAwait;

using Scriban;
using Scriban.Runtime;

namespace GenHTTP.Modules.Scriban.Providers
{

    public sealed class ScribanRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private Template? _Template;

        #region Get-/Setters

        public ChangeTrackingResource TemplateProvider { get; }

        #endregion

        #region Initialization

        public ScribanRenderer(IResource templateProvider)
        {
            TemplateProvider = new(templateProvider);
        }

        #endregion

        #region Functionality

        public async ValueTask<string> RenderAsync(T model)
        {
            var template = await GetTemplateAsync();

            var obj = new ScriptObject();

            obj.Import(model);
            obj.SetValue("route", new RoutingMethod(model), true);

            return await template.RenderAsync(obj);
        }

        private async PooledValueTask<Template> GetTemplateAsync()
        {
            if (_Template is null || await TemplateProvider.HasChanged())
            {
                _Template = await LoadTemplate();
            }

            return _Template!;
        }

        private async PooledValueTask<Template> LoadTemplate()
        {
            return Template.Parse(await TemplateProvider.GetResourceAsStringAsync());
        }

        #endregion

    }

}
