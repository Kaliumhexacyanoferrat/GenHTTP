using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Pages.Combined;

namespace GenHTTP.Modules.Markdown
{

    public static class PageExtensions
    {

        public static CombinedPageBuilder AddMarkdown(this CombinedPageBuilder builder, IBuilder<IResource> templateProvider)
        {
            return builder.AddMarkdown(templateProvider.Build());
        }

        public static CombinedPageBuilder AddMarkdown(this CombinedPageBuilder builder, IResource fileProvider)
        {
            return builder.Add(new MarkdownRenderer<IModel>(fileProvider));
        }

    }

}
