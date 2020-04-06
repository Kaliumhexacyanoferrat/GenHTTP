using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Core;

using RazorEngineCore;

namespace GenHTTP.Modules.Razor
{

    public class RazorRenderer<T> : IRenderer<T> where T : class, IBaseModel
    {
        private static RazorEngine _Engine = new RazorEngine();

        private RazorEngineCompiledTemplate<RazorEngineTemplateBase<T>>? _Template;

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

        private RazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> GetTemplate()
        {
            if (TemplateProvider.AllowCache)
            {
                return _Template ?? (_Template = LoadTemplate());
            }

            return LoadTemplate();
        }

        private RazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> LoadTemplate()
        {
            return _Engine.Compile<RazorEngineTemplateBase<T>>(TemplateProvider.GetResourceAsString(), (builder) =>
            {
                builder.AddAssemblyReference(Assembly.GetCallingAssembly());
            });
        }

        #endregion

    }

}
