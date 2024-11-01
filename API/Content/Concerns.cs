namespace GenHTTP.Api.Content;

/// <summary>
/// Utility class to work with concerns.
/// </summary>
public static class Concerns
{

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
