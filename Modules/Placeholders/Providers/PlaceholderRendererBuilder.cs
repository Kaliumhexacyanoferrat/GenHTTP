using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PlaceholderRendererBuilder<T> : IBuilder<IRenderer<T>> where T : class, IBaseModel
    {
        private IResource? _TemplateProvider;

        #region Functionality

        public PlaceholderRendererBuilder<T> TemplateProvider(IResource templateProvider)
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
