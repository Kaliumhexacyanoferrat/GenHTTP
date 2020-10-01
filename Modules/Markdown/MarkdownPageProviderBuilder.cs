using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Markdown
{
    public class MarkdownPageProviderBuilder<T> : IHandlerBuilder where T : PageModel
    {
        private IResourceProvider? _FileProvider;
        private string? _Title;

        public MarkdownPageProviderBuilder<T> File(IResourceProvider fileProvider)
        {
            _FileProvider = fileProvider;
            return this;
        }

        public MarkdownPageProviderBuilder<T> Title(string title)
        {
            _Title = title;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_FileProvider == null)
            {
                throw new BuilderMissingPropertyException("File Provider");
            }

            return new MarkdownPageProvider<T>(parent, _FileProvider, _Title);
        }
    }
}