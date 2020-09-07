using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Scriban.Providers;

namespace GenHTTP.Modules.Scriban
{

    public static class ModScriban
    {

        public static ScribanRendererBuilder<T> Template<T>(IBuilder<IResourceProvider> templateProvider) where T : class, IBaseModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static ScribanRendererBuilder<T> Template<T>(IResourceProvider templateProvider) where T : class, IBaseModel
        {
            return new ScribanRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static ScribanPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static ScribanPageProviderBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new ScribanPageProviderBuilder<PageModel>().Template(templateProvider).Model((r, h) => new PageModel(r, h));
        }

        public static ScribanPageProviderBuilder<T> Page<T>(IBuilder<IResourceProvider> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static ScribanPageProviderBuilder<T> Page<T>(IResourceProvider templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new ScribanPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
