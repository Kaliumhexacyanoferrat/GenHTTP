using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Pages.Combined
{

    public class CombinedPageBuilder : IHandlerBuilder<CombinedPageBuilder>, IContentInfoBuilder<CombinedPageBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly List<PageFragment> _Fragments = new();

        private readonly PageAdditions _Additions = PageAdditions.Create();

        #region Supporting data structures

        private class WrappedRenderer<T> : IRenderer<IModel> where T : class, IModel
        {

            private readonly IRenderer<T> _Renderer;

            public WrappedRenderer(IRenderer<T> renderer)
            {
                _Renderer = renderer;
            }

            public ValueTask<ulong> CalculateChecksumAsync() => _Renderer.CalculateChecksumAsync();

            public ValueTask PrepareAsync() => _Renderer.PrepareAsync();

            public ValueTask<string> RenderAsync(IModel model) => _Renderer.RenderAsync((T)model);

            public ValueTask RenderAsync(IModel model, Stream target) => _Renderer.RenderAsync((T)model, target);

        }

        #endregion

        #region Functionality

        /// <summary>
        /// Sets the description of the page.
        /// </summary>
        /// <param name="description">The description of the page</param>
        public CombinedPageBuilder Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        /// <summary>
        /// Sets the title of the page.
        /// </summary>
        /// <param name="description">The title of the page</param>
        public CombinedPageBuilder Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        /// <summary>
        /// Adds a dynamically rendered part to the page.
        /// </summary>
        /// <typeparam name="T">The type of the model to be rendered</typeparam>
        /// <param name="renderer">The renderer to be used</param>
        /// <param name="provider">The provider which will supply a template</param>
        public CombinedPageBuilder Add<T>(IRenderer<T> renderer, ModelProvider<T> provider) where T : class, IModel
        {
            async ValueTask<IModel> wrappedProvider(IRequest r, IHandler h) => await provider(r, h);

            Add(new PageFragment(new WrappedRenderer<T>(renderer), wrappedProvider));
            return this;
        }

        /// <summary>
        /// Adds a dynamically rendered part to the page.
        /// </summary>
        /// <param name="renderer">The renderer to be used</param>
        public CombinedPageBuilder Add(IRenderer<IModel> renderer)
        {
            Add(new PageFragment(renderer, (r, h) => new(new BasicModel(r, h))));
            return this;
        }

        /// <summary>
        /// Adds static content to the page.
        /// </summary>
        /// <param name="content">The content to be added as a part</param>
        public CombinedPageBuilder Add(string content) => Add(new TextRenderer(content), (r, h) => new(new BasicModel(r, h)));

        /// <summary>
        /// Adds a dynamic fragment to the page.
        /// </summary>
        /// <param name="fragment">The fragment to be added</param>
        public CombinedPageBuilder Add(PageFragment fragment)
        {
            _Fragments.Add(fragment);
            return this;
        }

        public CombinedPageBuilder AddScript(ScriptReference reference)
        {
            _Additions.Scripts.Add(reference);
            return this;
        }

        public CombinedPageBuilder AddStyle(StyleReference reference)
        {
            _Additions.Styles.Add(reference);
            return this;
        }

        public CombinedPageBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new CombinedPageProvider(p, _Info.Build(), _Additions, _Fragments));
        }

        #endregion

    }

}
