using System;
using System.Collections.Generic;
using System.Text;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Scriban
{

    public static class ModScriban
    {

        private class ImplicitModelProvider : IPageProvider<PageModel>
        {

            public PageModel GetModel(IHttpRequest request, IHttpResponse response)
            {
                return new PageModel(request, response);
            }

        }

        public static ScribanRendererBuilder Template(IBuilder<IResourceProvider> templateProvider)
        {
            return Template(templateProvider.Build());
        }

        public static ScribanRendererBuilder Template(IResourceProvider templateProvider)
        {
            return new ScribanRendererBuilder().TemplateProvider(templateProvider);
        }

        public static ScribanPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static ScribanPageProviderBuilder<PageModel> Page(IResourceProvider templateProvider)
        {
            return new ScribanPageProviderBuilder<PageModel>().Template(templateProvider).Model(new ImplicitModelProvider());
        }

        public static ScribanPageProviderBuilder<T> Page<T>(IBuilder<IResourceProvider> templateProvider, IPageProvider<T> modelProvider) where T : PageModel
        {
            return Page<T>(templateProvider.Build(), modelProvider);
        }

        public static ScribanPageProviderBuilder<T> Page<T>(IResourceProvider templateProvider, IPageProvider<T> modelProvider) where T : PageModel
        {
            return new ScribanPageProviderBuilder<T>().Template(templateProvider).Model(modelProvider);
        }
        
    }

}
