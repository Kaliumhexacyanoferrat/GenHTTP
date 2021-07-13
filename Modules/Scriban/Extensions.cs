using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Pages.Combined;
using GenHTTP.Modules.Scriban.Providers;

namespace GenHTTP.Modules.Scriban
{
    
    public static class Extensions
    {

        public static CombinedPageBuilder AddScriban(this CombinedPageBuilder builder, IBuilder<IResource> template) => AddScriban(builder, template.Build());

        public static CombinedPageBuilder AddScriban(this CombinedPageBuilder builder, IResource template)
        {
            builder.Add(new ScribanRenderer<IModel>(template));
            return builder;
        }

        public static CombinedPageBuilder AddScriban<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this CombinedPageBuilder builder, IBuilder<IResource> template, ModelProvider<T> modelProvider) where T : class, IModel => AddScriban<T>(builder, template.Build(), modelProvider);


        public static CombinedPageBuilder AddScriban<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this CombinedPageBuilder builder, IResource template, ModelProvider<T> modelProvider) where T : class, IModel
        {
            builder.Add(new ScribanRenderer<T>(template), modelProvider);
            return builder;
        }

    }

}
