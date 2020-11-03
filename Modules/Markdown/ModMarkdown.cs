using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Markdown
{

    public static class ModMarkdown
    {

        public static MarkdownPageProviderBuilder<PageModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static MarkdownPageProviderBuilder<PageModel> Page(IResource fileProvider)
        {
            return new MarkdownPageProviderBuilder<PageModel>().File(fileProvider);
        }

    }

}
