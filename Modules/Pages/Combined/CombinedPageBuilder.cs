using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Pages.Combined
{

    public class CombinedPageBuilder : IHandlerBuilder<CombinedPageBuilder>, IContentInfoBuilder<CombinedPageBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly List<PageFragment> _Fragments = new();

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
            _Fragments.Add(new((IRenderer<IModel>)renderer, (ModelProvider<IModel>)(object)provider)); // ToDo
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
