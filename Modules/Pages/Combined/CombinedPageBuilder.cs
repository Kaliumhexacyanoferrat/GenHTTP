using System.Collections.Generic;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Pages.Combined
{

    public class CombinedPageBuilder : IHandlerBuilder<CombinedPageBuilder>, IContentInfoBuilder<CombinedPageBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly List<PageFragment> _Fragments = new();

        #region Supporting data structures

        private class WrappedRenderer<T> : IRenderer<IModel> where T : class, IModel
        {

            private IRenderer<T> _Renderer;

            public WrappedRenderer(IRenderer<T> renderer)
            {
                _Renderer = renderer;
            }

            public ValueTask<ulong> CalculateChecksumAsync() => _Renderer.CalculateChecksumAsync();

            public ValueTask PrepareAsync() => _Renderer.PrepareAsync();

            public ValueTask<string> RenderAsync(IModel model) => _Renderer.RenderAsync((T)model);

        }

        #endregion

        #region Functionality

        public CombinedPageBuilder Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public CombinedPageBuilder Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public CombinedPageBuilder Add<T>(IRenderer<T> renderer, ModelProvider<T> provider) where T : class, IModel
        {
            async ValueTask<IModel> wrappedProvider(IRequest r, IHandler h) => await provider(r, h);

            Add(new PageFragment(new WrappedRenderer<T>(renderer), wrappedProvider));
            return this;
        }

        public CombinedPageBuilder Add(IRenderer<IModel> renderer)
        {
            _Fragments.Add(new(renderer, (r, h) => new(new BasicModel(r, h))));
            return this;
        }

        public CombinedPageBuilder Add(string content) => Add(new TextRenderer(content), (r, h) => new(new BasicModel(r, h)));

        public CombinedPageBuilder Add(PageFragment fragment)
        {
            _Fragments.Add(fragment);
            return this;
        }

        public CombinedPageBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new CombinedPageProvider(p, _Info.Build(), _Fragments));
        }

        #endregion

    }

}
