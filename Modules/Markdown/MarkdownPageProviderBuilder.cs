using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownPageProviderBuilder<T> : IHandlerBuilder<MarkdownPageProviderBuilder<T>>, IContentInfoBuilder<MarkdownPageProviderBuilder<T>> where T : PageModel
    {
        private IResourceProvider? _FileProvider;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        private readonly ContentInfoBuilder _Info = new ContentInfoBuilder();

        #region Functionality

        public MarkdownPageProviderBuilder<T> File(IResourceProvider fileProvider)
        {
            _FileProvider = fileProvider;
            return this;
        }

        public MarkdownPageProviderBuilder<T> Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_FileProvider == null)
            {
                throw new BuilderMissingPropertyException("File Provider");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new MarkdownPageProvider<T>(p, _FileProvider, _Info.Build()));
        }

        #endregion

    }

}