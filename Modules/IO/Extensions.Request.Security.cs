using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class RequestSecurityExtensions
{

    /// <summary>
    /// Checks all remaining, non-routed segments of the request target for
    /// path traversal attacks and throws a provider exception if one
    /// is detected.
    /// </summary>
    /// <remarks>
    /// Denies dot segments ("." and "..") as well as segments that contain
    /// a path separator ("/", "\") or a null byte, either literally or
    /// percent-encoded.
    /// </remarks>
    /// <param name="target">The request target to be checked</param>
    public static void DenyPathTraversal(this IRequestTarget target)
    {
        var index = 0;

        PathSegment? segment;

        while ((segment = target.Next(index++)) != null)
        {
            if (IsForbidden(segment.Value.Bytes.Span))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Potential path traversal detected");
            }
        }
    }

    private static bool IsForbidden(ReadOnlySpan<byte> segment)
    {
        var dots = 0;
        var onlyDots = true;

        for (var i = 0; i < segment.Length; i++)
        {
            var c = segment[i];

            if (c == (byte)'%' && i + 2 < segment.Length && PercentEncoding.TryDecode(segment[i + 1], segment[i + 2], out var unescaped))
            {
                c = unescaped;
                i += 2;
            }

            if (c is (byte)'/' or (byte)'\\' or 0)
            {
                return true;
            }

            if (c == (byte)'.')
            {
                dots++;
            }
            else
            {
                onlyDots = false;
            }
        }

        return onlyDots && dots is 1 or 2;
    }

}
