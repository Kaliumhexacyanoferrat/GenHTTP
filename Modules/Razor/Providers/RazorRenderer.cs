using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Tracking;

using PooledAwait;

using RazorEngineCore;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly static RazorEngine _Engine = new();

        private IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>>? _Template;

        #region Get-/Setters

        public ChangeTrackingResource TemplateProvider { get; }

        #endregion

        #region Initialization

        public RazorRenderer(IResource templateProvider)
        {
            TemplateProvider = new(templateProvider);
        }

        #endregion

        #region Functionality

        public async ValueTask<string> RenderAsync(T model)
        {
            var template = await GetTemplate();

            return await template.RunAsync((instance) =>
            {
                instance.Model = model;
            });
        }

        private async PooledValueTask<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>>> GetTemplate()
        {
            if (_Template == null || await TemplateProvider.HasChanged())
            {
                _Template = await LoadTemplate();
            }

            return _Template!;
        }

        private async PooledValueTask<IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>>> LoadTemplate()
        {
            return await _Engine.CompileAsync<RazorEngineTemplateBase<T>>(await TemplateProvider.GetResourceAsStringAsync(), (builder) =>
            {
                builder.AddAssemblyReferenceByName("System.Collections");

                builder.AddAssemblyReference(typeof(HttpUtility));
                builder.AddUsing("System.Web");

                builder.AddAssemblyReference(Assembly.GetCallingAssembly());

                builder.AddAssemblyReferenceByName("GenHTTP.Api");
                builder.AddAssemblyReferenceByName("GenHTTP.Modules.Razor");

                builder.AddUsing("GenHTTP.Modules.Razor");
                builder.AddUsing("GenHTTP.Modules.Razor.Providers");
            });
        }

        #endregion

    }

}
