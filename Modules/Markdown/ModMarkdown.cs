using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Markdown
{

    public static class ModMarkdown
    {

        public static MarkdownPageProviderBuilder<IModel> Page(IBuilder<IResource> templateProvider)
        {
            return Page(templateProvider.Build());
        }

        public static MarkdownPageProviderBuilder<IModel> Page(IResource fileProvider)
        {
            return new MarkdownPageProviderBuilder<IModel>().File(fileProvider);
        }

    }

}
