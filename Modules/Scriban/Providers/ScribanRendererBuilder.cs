using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Scriban.Providers
{

    public class ScribanRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IBaseModel
    {
        protected IResource? _TemplateProvider;

        #region Functionality

        public ScribanRendererBuilder<T> TemplateProvider(IResource templateProvider)
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

            return new ScribanRenderer<T>(_TemplateProvider);
        }

        #endregion

    }

}
