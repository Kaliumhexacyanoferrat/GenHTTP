using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.Layouting;

public static class MultiSegmentSupport
{

    /// <summary>
    /// Adds multiple segments (e.g. /api/v1) to the layout and appends the given handler to the last one.
    /// </summary>
    /// <param name="builder">The layout to add the segments to</param>
    /// <param name="segments">The segments to be added</param>
    /// <param name="handler">The handler to be appended</param>
    /// <returns>The builder the segments have been added to</returns>
    /// <exception cref="InvalidOperationException">Thrown if there are incompatible handlers already registered at a segment</exception>
    public static LayoutBuilder Add(this LayoutBuilder builder, string[] segments, IHandlerBuilder handler)
    {
        if (segments.Length == 0)
        {
            return builder.Add(handler);
        }

        var current = builder;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            var segment = segments[i];

            if (current.RoutedHandlers.TryGetValue(segment, out var existing))
            {
                if (existing is LayoutBuilder existingLayout)
                {
                    current = existingLayout;
                }
                else
                {
                    throw new InvalidOperationException($"Existing segment '{segment}' is no layout");
                }
            }
            else
            {
                var newLayout = Layout.Create();

                current.Add(segment, newLayout);
                current = newLayout;
            }
        }

        current.Add(segments[^1], handler);

        return builder;
    }

}
