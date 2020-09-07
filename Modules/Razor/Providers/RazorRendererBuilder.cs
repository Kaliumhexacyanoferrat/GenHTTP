using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IBaseModel
    {
        protected IResourceProvider? _TemplateProvider;

        #region Functionality

        public RazorRendererBuilder<T> TemplateProvider(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public IRenderer<T> Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new RazorRenderer<T>(_TemplateProvider);
        }

        #endregion

    }

}
