using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IBaseModel
    {
        private IResourceProvider? _TemplateProvider;

        #region Functionality

        public PlaceholderRendererBuilder<T> TemplateProvider(IResourceProvider templateProvider)
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

            return new PlaceholderRender<T>(_TemplateProvider);
        }

        #endregion

    }

}
