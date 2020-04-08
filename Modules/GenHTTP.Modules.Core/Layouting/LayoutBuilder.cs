using GenHTTP.Api.Content;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.Layouting
{

    public class LayoutBuilder : IHandlerBuilder
    {
        private IHandlerBuilder? _Index, _Fallback;

        #region Get-/Setters

        private Dictionary<string, IHandlerBuilder> Folders { get; }

        private Dictionary<string, IHandlerBuilder> Files { get; }

        #endregion

        #region Initialization

        public LayoutBuilder()
        {
            Folders = new Dictionary<string, IHandlerBuilder>();
            Files = new Dictionary<string, IHandlerBuilder>();
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

        // ToDo: Rework again into "Add" and use a specific interface for something like IDirectContent

        public LayoutBuilder Section(string name, IHandlerBuilder handler)
        {
            Folders.Add(name, handler);
            return this;
        }

        public LayoutBuilder File(string name, IHandlerBuilder handler)
        {
            Files.Add(name, handler);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return new LayoutRouter(parent, Folders, Files, _Index, _Fallback);
        }

        #endregion

    }

}
