using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IBaseModel
    {
        protected IResource? _TemplateProvider;

        #region Functionality

        public RazorRendererBuilder<T> TemplateProvider(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public IRenderer<T> Build()
        {
            if (_TemplateProvider is null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new RazorRenderer<T>(_TemplateProvider);
        }

        #endregion

    }

}
