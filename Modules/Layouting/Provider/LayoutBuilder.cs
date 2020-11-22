using System.Collections.Generic;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Layouting.Provider
{

    public sealed class LayoutBuilder : IHandlerBuilder<LayoutBuilder>
    {
        private IHandlerBuilder? _Index, _Fallback;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Get-/Setters

        private Dictionary<string, IHandlerBuilder> Handlers { get; }

        #endregion

        #region Initialization

        public LayoutBuilder()
        {
            Handlers = new Dictionary<string, IHandlerBuilder>();
        }

        #endregion

        #region Functionality

        public LayoutBuilder Index(IHandlerBuilder handler)
        {
            _Index = handler;
            return this;
        }

        public LayoutBuilder Fallback(IHandlerBuilder handler)
        {
            _Fallback = handler;
            return this;
        }

        public LayoutBuilder Add(string name, IHandlerBuilder handler)
        {
            Handlers.Add(name, handler);
            return this;
        }

        public LayoutBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new LayoutRouter(p, Handlers, _Index, _Fallback));
        }

        #endregion

    }

}
