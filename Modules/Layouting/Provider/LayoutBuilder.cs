using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Layouting.Provider
{

    public sealed class LayoutBuilder : IHandlerBuilder<LayoutBuilder>
    {
        private IHandlerBuilder? _Index;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Get-/Setters

        private Dictionary<string, IHandlerBuilder> RoutedHandlers { get; }

        private List<IHandlerBuilder> RootHandlers { get; }

        #endregion

        #region Initialization

        public LayoutBuilder()
        {
            RoutedHandlers = new();
            RootHandlers = new();
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Sets the handler which should be invoked to provide
        /// the index of the layout.
        /// </summary>
        /// <param name="handler">The handler used for the index of the layout</param>
        public LayoutBuilder Index(IHandlerBuilder handler)
        {
            _Index = handler;
            return this;
        }

        [Obsolete("Deprecated, use Add() instead")]
        public LayoutBuilder Fallback(IHandlerBuilder handler) => Add(handler);

        /// <summary>
        /// Adds a handler that will be invoked for all URLs below
        /// the specified path segment.
        /// </summary>
        /// <param name="name">The name of the path segment to be handled</param>
        /// <param name="handler">The handler which will handle the segment</param>
        /// <remarks>
        /// Can be used to provide one or multiple fallback handlers for the layout.
        /// Fallback handlers will be executed in the order they have been added
        /// to the layout.
        /// </remarks>
        public LayoutBuilder Add(string name, IHandlerBuilder handler)
        {
            RoutedHandlers.Add(name, handler);
            return this;
        }

        /// <summary>
        /// Adds a handler on root level that will be invoked if neither a
        /// path segment has been detected nor the index has been invoked.
        /// </summary>
        /// <param name="handler">The root level handler to be added</param>
        public LayoutBuilder Add(IHandlerBuilder handler)
        {
            RootHandlers.Add(handler);
            return this;
        }

        public LayoutBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new LayoutRouter(p, RoutedHandlers, RootHandlers, _Index));
        }

        #endregion

    }

}
