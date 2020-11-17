using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Razor.Providers;

namespace GenHTTP.Modules.Razor
{

    public static class ModRazor
    {

        public static RazorRendererBuilder<T> Template<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider) where T : class, IBaseModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static RazorRendererBuilder<T> Template<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider) where T : class, IBaseModel
        {
            return new RazorRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static RazorPageProviderBuilder<PageModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static RazorPageProviderBuilder<PageModel> Page(IResource templateProvider)
        {
            return new RazorPageProviderBuilder<PageModel>().Template(templateProvider).Model((r, h) => new PageModel(r, h));
        }

        public static RazorPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static RazorPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new RazorPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
