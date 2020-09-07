using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;

using RazorEngineCore;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private readonly static RazorEngine _Engine = new RazorEngine();

        private IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>>? _Template;

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }

        #endregion

        #region Initialization

        public RazorRenderer(IResourceProvider templateProvider)
        {
            TemplateProvider = templateProvider;
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
            if (TemplateProvider.AllowCache)
            {
                return _Template ?? (_Template = LoadTemplate());
            }

            return LoadTemplate();
        }

        private IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> LoadTemplate()
        {
            return _Engine.Compile<RazorEngineTemplateBase<T>>(TemplateProvider.GetResourceAsString(), (builder) =>
            {
                builder.AddAssemblyReference(Assembly.GetCallingAssembly());
                builder.AddAssemblyReference(Assembly.GetExecutingAssembly());

                builder.AddUsing("GenHTTP.Modules.Razor");
                builder.AddUsing("GenHTTP.Modules.Razor.Providers");
            });
        }

        #endregion

    }

}
