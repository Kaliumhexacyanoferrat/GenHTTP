using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core
{

    public static class Placeholders
    {
        
        public static PlaceholderRendererBuilder Template(IBuilder<IResourceProvider> templateProvider)
        {
            return Template(templateProvider.Build());
        }

        public static PlaceholderRendererBuilder Template(IResourceProvider templateProvider)
        {
            return new PlaceholderRendererBuilder().TemplateProvider(templateProvider);
        }

        public static PlaceholderPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static PlaceholderPageProviderBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new PlaceholderPageProviderBuilder<PageModel>().Template(templateProvider).Model(r => new PageModel(r));
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IBuilder<IResourceProvider> templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static PlaceholderPageProviderBuilder<T> Page<T>(IResourceProvider templateProvider, ModelProvider<T> modelProvider) where T : PageModel
        {
            return new PlaceholderPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
