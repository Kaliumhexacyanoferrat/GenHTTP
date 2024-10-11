namespace GenHTTP.Api.Content;

/// <summary>
///     Utility class to work with concerns.
/// </summary>
public static class Concerns
{

    /// <summary>
    ///     Creates a handler chain to wrap the specified handler into the
    ///     specified concerns.
    /// </summary>
    /// <remarks>
    ///     Use this utility within the handler builders to add concerns
    ///     to the resulting handler instance. The last concern added
    ///     to the list of concerns will be the root handler returned by
    ///     this method.
    /// </remarks>
    /// <param name="parent">The parent handler of the chain</param>
    /// <param name="concerns">The concerns that should be wrapped around the inner handler</param>
    /// <param name="factory">The logic creating the actual handler to be chained</param>
    /// <returns>The outermost handler or root of the chain</returns>
    public static IHandler Chain(IHandler parent, IEnumerable<IConcernBuilder> concerns, Func<IHandler, IHandler> factory)
    {
        var stack = new Stack<IConcernBuilder>(concerns);

        return Chain(parent, stack, factory);
    }

    private static IHandler Chain(IHandler parent, Stack<IConcernBuilder> remainders, Func<IHandler, IHandler> factory)
    {
        if (remainders.Count > 0)
        {
            var concern = remainders.Pop();

            return concern.Build(parent, parent => Chain(parent, remainders, factory));
        }

        return factory(parent);
    }
}
