using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Pages.Combined;

namespace GenHTTP.Modules.Razor
{

    public static class PageExtensions
    {

        public static CombinedPageBuilder AddRazor(this CombinedPageBuilder builder, IBuilder<IResource> templateProvider)
        {
            return builder.AddRazor(templateProvider.Build());
        }

        public static CombinedPageBuilder AddRazor(this CombinedPageBuilder builder, IResource templateProvider)
        {
            return builder.Add(ModRazor.Template<BasicModel>(templateProvider).Build(), (r, h) => new ValueTask<BasicModel>(new BasicModel(r, h)));
        }

        public static CombinedPageBuilder AddRazor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this CombinedPageBuilder builder, IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return builder.AddRazor(templateProvider.Build(), modelProvider);
        }

        public static CombinedPageBuilder AddRazor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this CombinedPageBuilder builder, IResource templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return builder.Add(ModRazor.Template<T>(templateProvider).Build(), modelProvider);
        }

    }

}
