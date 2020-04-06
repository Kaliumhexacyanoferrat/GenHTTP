using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanRendererBuilder : IBuilder<IRenderer<TemplateModel>>
    {
        protected IResourceProvider? _TemplateProvider;

        #region Functionality

        public ScribanRendererBuilder TemplateProvider(IResourceProvider templateProvider)
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

            return new ScribanRenderer<TemplateModel>(_TemplateProvider);
        }

        #endregion

    }

}
