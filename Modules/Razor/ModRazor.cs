using System;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Razor.Providers;

namespace GenHTTP.Modules.Razor
{

    public static class ModRazor
    {

        public static RazorRendererBuilder<T> Template<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider) where T : class, IModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static RazorRendererBuilder<T> Template<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider) where T : class, IModel
        {
            return new RazorRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static RazorPageProviderBuilder<IModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static RazorPageProviderBuilder<IModel> Page(IResource templateProvider)
        {
            return new RazorPageProviderBuilder<IModel>().Template(templateProvider).Model((r, h) => new BasicModel(r, h));
        }

        public static RazorPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static RazorPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IBuilder<IResource> templateProvider, Func<IRequest, IHandler, T> modelProvider) where T : class, IModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static RazorPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return new RazorPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

        public static RazorPageProviderBuilder<T> Page<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IResource templateProvider, Func<IRequest, IHandler, T> modelProvider) where T : class, IModel
        {
            return new RazorPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
