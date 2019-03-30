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

        public static PlaceholderPageBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static PlaceholderPageBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new PlaceholderPageBuilder<PageModel>().Template(templateProvider).Model(new ImplicitModelProvider());
        }

        public static PlaceholderPageBuilder<T> Page<T>(IBuilder<IResourceProvider> templateProvider, IPageProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static PlaceholderPageBuilder<T> Page<T>(IResourceProvider templateProvider, IPageProvider<T> modelProvider) where T : PageModel
        {
            return new PlaceholderPageBuilder<T>().Template(templateProvider).Model(modelProvider);
        }

    }

}
