﻿using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Placeholders.Providers;

namespace GenHTTP.Modules.Placeholders
{

    public static class Placeholders
    {

        public static PlaceholderRendererBuilder<T> Template<T>(IBuilder<IResource> templateProvider) where T : class, IModel
        {
            return Template<T>(templateProvider.Build());
        }

        public static PlaceholderRendererBuilder<T> Template<T>(IResource templateProvider) where T : class, IModel
        {
            return new PlaceholderRendererBuilder<T>().TemplateProvider(templateProvider);
        }

        public static PlaceholderPageProviderBuilder<IModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static PlaceholderPageProviderBuilder<IModel> Page(IResource templateProvider)
        {
            return new PlaceholderPageProviderBuilder<IModel>().Template(templateProvider).Model((r, h) => new BasicModel(r, h));
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IBuilder<IResource> templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return Page(templateProvider.Build(), modelProvider);
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IResource templateProvider, ModelProvider<T> modelProvider) where T : class, IModel
        {
            return new PlaceholderPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
