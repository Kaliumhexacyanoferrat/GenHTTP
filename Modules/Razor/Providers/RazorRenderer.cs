using System.Reflection;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

using RazorEngineCore;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly static RazorEngine _Engine = new RazorEngine();

        private IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>>? _Template;

        #region Get-/Setters

        public CachedResource TemplateProvider { get; }

        #endregion

        #region Initialization

        public RazorRenderer(IResource templateProvider)
        {
            TemplateProvider = new CachedResource(templateProvider);
        }

        #endregion

        #region Functionality

        public string Render(T model)
        {
            var template = GetTemplate();

            return template.Run((instance) =>
            {
                instance.Model = model;
            });
        }

        private IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> GetTemplate()
        {
            if (TemplateProvider.Changed)
            {
                _Template = LoadTemplate();
            }

            return _Template!;
        }

        private IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> LoadTemplate()
        {
            return _Engine.Compile<RazorEngineTemplateBase<T>>(TemplateProvider.GetResourceAsString(), (builder) =>
            {
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
