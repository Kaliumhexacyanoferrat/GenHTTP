using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Scriban.Providers;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Scriban
{

    public static class ModScriban
    {

        public static ScribanRendererBuilder<T> Template<T>(IBuilder<IResource> templateProvider) where T : class, IBaseModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static ScribanRendererBuilder<T> Template<T>(IResource templateProvider) where T : class, IBaseModel
        {
            return new ScribanRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static ScribanPageProviderBuilder<PageModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static ScribanPageProviderBuilder<PageModel> Page(IResource templateProvider)
        {
            return new ScribanPageProviderBuilder<PageModel>().Template(templateProvider).Model((r, h) => new PageModel(r, h));
        }

        public static ScribanPageProviderBuilder<T> Page<T>(IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static ScribanPageProviderBuilder<T> Page<T>(IResource templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new ScribanPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
