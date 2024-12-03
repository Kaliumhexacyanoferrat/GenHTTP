using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Layouting.Provider;

public sealed class LayoutBuilder : IHandlerBuilder<LayoutBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private IHandlerBuilder? _Index;

    #region Get-/Setters

    private Dictionary<string, IHandlerBuilder> RoutedHandlers { get; }

    private List<IHandlerBuilder> RootHandlers { get; }

    #endregion

    #region Initialization

    public LayoutBuilder()
    {
        RoutedHandlers = [];
        RootHandlers = [];
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Sets the handler which should be invoked to provide
    /// the index of the layout.
    /// </summary>
    /// <param name="handler">The handler used for the index of the layout</param>
    public LayoutBuilder Index(IHandler handler) => Index(new HandlerWrapper(handler));

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

    /// <summary>
    /// Adds a handler that will be invoked for all URLs below
    /// the specified path segment.
    /// </summary>
    /// <param name="name">The name of the path segment to be handled</param>
    /// <param name="handler">The handler which will handle the segment</param>
    public LayoutBuilder Add(string name, IHandler handler) => Add(new HandlerWrapper(handler));

    /// <summary>
    /// Adds a handler that will be invoked for all URLs below
    /// the specified path segment.
    /// </summary>
    /// <param name="name">The name of the path segment to be handled</param>
    /// <param name="handler">The handler which will handle the segment</param>
    public LayoutBuilder Add(string name, IHandlerBuilder handler)
    {
        if (name.Contains('/'))
        {
            throw new ArgumentException("Path seperators are not allowed in the name of the segment.", nameof(name));
        }

        RoutedHandlers.Add(name, handler);
        return this;
    }

    /// <summary>
    /// Adds a handler on root level that will be invoked if neither a
    /// path segment has been detected nor the index has been invoked.
    /// </summary>
    /// <param name="handler">The root level handler to be added</param>
    /// <remarks>
    /// Can be used to provide one or multiple fallback handlers for the layout.
    /// Fallback handlers will be executed in the order they have been added
    /// to the layout.
    /// </remarks>
    public LayoutBuilder Add(IHandler handler) => Add(new HandlerWrapper(handler));

    /// <summary>
    /// Adds a handler on root level that will be invoked if neither a
    /// path segment has been detected nor the index has been invoked.
    /// </summary>
    /// <param name="handler">The root level handler to be added</param>
    /// <remarks>
    /// Can be used to provide one or multiple fallback handlers for the layout.
    /// Fallback handlers will be executed in the order they have been added
    /// to the layout.
    /// </remarks>
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

    public IHandler Build()
    {
        var routed = RoutedHandlers.ToDictionary(kv => kv.Key, kv => kv.Value.Build());
        var root = RootHandlers.Select(h => h.Build()).ToList();

        return Concerns.Chain(_Concerns, new LayoutRouter(routed, root, _Index?.Build()));
    }

    #endregion

}
