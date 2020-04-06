using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Razor
{

    public static class ModRazor
    {

        public static RazorRendererBuilder Template(IBuilder<IResourceProvider> templateProvider)
        {
            return Template(templateProvider.Build());
        }

        public static RazorRendererBuilder Template(IResourceProvider templateProvider)
        {
            return new RazorRendererBuilder().TemplateProvider(templateProvider);
        }

        public static RazorPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static RazorPageProviderBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new RazorPageProviderBuilder<PageModel>().Template(templateProvider).Model(r => new PageModel(r));
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
