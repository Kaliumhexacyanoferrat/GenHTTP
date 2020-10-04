using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Markdown
{

    public static class ModMarkdown
    {

        public static MarkdownPageProviderBuilder<PageModel> Page(IBuilder<IResourceProvider> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static MarkdownPageProviderBuilder<PageModel> Page(IResourceProvider fileProvider)
        {
            return new MarkdownPageProviderBuilder<PageModel>().File(fileProvider);
        }

    }

}
