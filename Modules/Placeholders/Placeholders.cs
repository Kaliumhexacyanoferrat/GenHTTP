using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.Placeholders.Providers;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Placeholders
{

    public static class Placeholders
    {

        public static PlaceholderRendererBuilder<T> Template<T>(IBuilder<IResource> templateProvider) where T : class, IBaseModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static PlaceholderRendererBuilder<T> Template<T>(IResource templateProvider) where T : class, IBaseModel
        {
            return new PlaceholderRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static PlaceholderPageProviderBuilder<PageModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static PlaceholderPageProviderBuilder<PageModel> Page(IResource templateProvider)
        {
            return new PlaceholderPageProviderBuilder<PageModel>().Template(templateProvider).Model((r, h) => new PageModel(r, h));
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page(templateProvider.Build(), modelProvider);
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IResource templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new PlaceholderPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
