using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Placeholders.Providers;

namespace GenHTTP.Modules.Placeholders
{

    public static class Placeholders
    {

        public static PlaceholderRendererBuilder<T> Template<T>(IBuilder<IResourceProvider> templateProvider) where T : class, IBaseModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static PlaceholderRendererBuilder<T> Template<T>(IResourceProvider templateProvider) where T : class, IBaseModel
        {
            return new PlaceholderRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static PlaceholderPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static PlaceholderPageProviderBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new PlaceholderPageProviderBuilder<PageModel>().Template(templateProvider).Model((r, h) => new PageModel(r, h));
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IBuilder<IResourceProvider> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page(templateProvider.Build(), modelProvider);
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IResourceProvider templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new PlaceholderPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
