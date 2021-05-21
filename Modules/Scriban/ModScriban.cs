using System;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Scriban.Providers;

namespace GenHTTP.Modules.Scriban
{

    public static class ModScriban
    {

        public static ScribanRendererBuilder<T> Template<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider) where T : class, IModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static ScribanRendererBuilder<T> Template<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider) where T : class, IModel
        {
            return new ScribanRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static ScribanPageProviderBuilder<IModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static ScribanPageProviderBuilder<IModel> Page(IResource templateProvider)
        {
            return new ScribanPageProviderBuilder<IModel>().Template(templateProvider).Model((r, h) => new BasicModel(r, h));
        }

        public static ScribanPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static ScribanPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider, Func<IRequest, IHandler, T> modelProvider) where T : class, IModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static ScribanPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return new ScribanPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

        public static ScribanPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider, Func<IRequest, IHandler, T> modelProvider) where T : class, IModel
        {
            return new ScribanPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
