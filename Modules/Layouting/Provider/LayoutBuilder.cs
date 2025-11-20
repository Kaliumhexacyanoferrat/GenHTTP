using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Layouting.Provider;

public sealed class LayoutBuilder : IHandlerBuilder<LayoutBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IHandlerBuilder? _index;

    #region Get-/Setters

    internal Dictionary<string, IHandlerBuilder> RoutedHandlers { get; } = [];

    internal List<IHandlerBuilder> RootHandlers { get; } = [];

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
        _index = handler;
        return this;
    }

    /// <summary>
    /// Adds a handler that will be invoked for all URLs below
    /// the specified path segment.
    /// </summary>
    /// <param name="name">The name of the path segment to be handled</param>
    /// <param name="handler">The handler which will handle the segment</param>
    public LayoutBuilder Add(string name, IHandler handler) => Add(name, new HandlerWrapper(handler));

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
            return this.Add(name.Split('/', StringSplitOptions.RemoveEmptyEntries), handler);
        }

        if (!RoutedHandlers.TryAdd(name, handler))
        {
            throw new InvalidOperationException($"A segment with the name '{name}' has already been added to the layout");
        }

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
        _concerns.Add(concern);
        return this;
    }

    /// <summary>
    /// Creates a new layout and registers it at the given path.
    /// </summary>
    /// <param name="section">The path of the segment to be added</param>
    /// <returns>The newly created segment</returns>
    public LayoutBuilder AddSegment(string segment)
    {
        var child = Layout.Create();

        Add(segment, child);

        return child;
    }

    public IHandler Build()
    {
        var routed = RoutedHandlers.ToDictionary(kv => kv.Key, kv => kv.Value.Build());
        var root = RootHandlers.Select(h => h.Build()).ToList();

        return Concerns.Chain(_concerns, new LayoutRouter(routed, root, _index?.Build()));
    }

    #endregion

}
