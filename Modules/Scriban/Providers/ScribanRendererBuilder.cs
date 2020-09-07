using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Scriban.Providers
{

    public class ScribanRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IBaseModel
    {
        protected IResourceProvider? _TemplateProvider;

        #region Functionality

        public ScribanRendererBuilder<T> TemplateProvider(IResourceProvider templateProvider)
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

            return new ScribanRenderer<T>(_TemplateProvider);
        }

        #endregion

    }

}
