using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Razor.Providers;

namespace GenHTTP.Modules.Razor
{

    public static class ModRazor
    {

        public static RazorRendererBuilder<T> Template<T>(IBuilder<IResourceProvider> templateProvider) where T : class, IBaseModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static RazorRendererBuilder<T> Template<T>(IResourceProvider templateProvider) where T : class, IBaseModel
        {
            return new RazorRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static RazorPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static RazorPageProviderBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new RazorPageProviderBuilder<PageModel>().Template(templateProvider).Model((r, h) => new PageModel(r, h));
        }

        public static RazorPageProviderBuilder<T> Page<T>(IBuilder<IResourceProvider> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static RazorPageProviderBuilder<T> Page<T>(IResourceProvider templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new RazorPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
