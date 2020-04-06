using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Razor
{

    public class RazorRendererBuilder : IBuilder<IRenderer<TemplateModel>>
    {
        protected IResourceProvider? _TemplateProvider;

        #region Functionality

        public RazorRendererBuilder TemplateProvider(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public IRenderer<TemplateModel> Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new RazorRenderer<TemplateModel>(_TemplateProvider);
        }

        #endregion

    }

}
