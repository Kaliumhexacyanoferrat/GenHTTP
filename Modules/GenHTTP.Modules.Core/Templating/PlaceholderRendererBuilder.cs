using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderRendererBuilder : IBuilder<IRenderer<TemplateModel>>
    {
        private IResourceProvider? _TemplateProvider;

        #region Functionality

        public PlaceholderRendererBuilder TemplateProvider(IResourceProvider templateProvider)
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

            return new PlaceholderRender<TemplateModel>(_TemplateProvider);
        }

        #endregion

    }

}
