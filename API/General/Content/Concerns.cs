namespace GenHTTP.Api.Content;

/// <summary>
/// Utility class to work with concerns.
/// </summary>
public static class Concerns
{

    /// <summary>
    /// Embeds the given handler into a handler chain build from
    /// the given concerns.
    /// </summary>
    /// <param name="concerns">The concerns the handler should be chained to</param>
    /// <param name="handler">The handler to be chained</param>
    /// <returns>Returns to outermost handler of the new chain</returns>
    public static IHandler Chain(IEnumerable<IConcernBuilder> concerns, IHandler handler)
    {
        var content = handler;

        foreach (var concern in concerns)
        {
            content = concern.Build(content);
        }

        return content;
    }

}
