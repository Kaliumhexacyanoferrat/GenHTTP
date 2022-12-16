using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;
using GenHTTP.Modules.IO.Streaming;

using Scriban;
using Scriban.Runtime;

namespace GenHTTP.Modules.Scriban.Providers
{

    public sealed class ScribanRenderer<T> : IRenderer<T> where T : class, IModel
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
        
        public ValueTask<ulong> CalculateChecksumAsync() => TemplateProvider.CalculateChecksumAsync();

        public async ValueTask<string> RenderAsync(T model)
        {
            var template = await GetTemplateAsync();

            var obj = new ScriptObject();

            obj.Import(model);
            obj.SetValue("route", new RoutingMethod(model), true);

            return await template.RenderAsync(obj);
        }

        public ValueTask RenderAsync(T model, Stream target) => this.RenderToStream(model, target);

        public async ValueTask PrepareAsync()
        {
            if (_Template is null)
            {
                _Template = await LoadTemplate();
            }
        }

        private async ValueTask<Template> GetTemplateAsync()
        {
            if (_Template is null || await TemplateProvider.HasChanged())
            {
                _Template = await LoadTemplate();
            }

            return _Template!;
        }

        private async ValueTask<Template> LoadTemplate()
        {
            return Template.Parse(await TemplateProvider.GetResourceAsStringAsync());
        }

        #endregion

    }

}
