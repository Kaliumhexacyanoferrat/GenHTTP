using System.Runtime.CompilerServices;

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
    /// <param name="target">The request target to be checked</param>
    public static void DenyPathTraversal(this IRequestTarget target)
    {
        var index = 0;

        PathSegment? segment;

        while ((segment = target.Next(index++)) != null)
        {
            if (IsDotSegment(segment.Value.Bytes.Span))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Potential path traversal detected");
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDotSegment(ReadOnlySpan<byte> segment)
    {
        var dots = 0;

        for (var i = 0; i < segment.Length;)
        {
            if (segment[i] == (byte)'.')
            {
                dots++;
                i++;
            }
            else if (segment[i] == (byte)'%' && i + 2 < segment.Length && segment[i + 1] == (byte)'2' && (segment[i + 2] == (byte)'e' || segment[i + 2] == (byte)'E'))
            {
                dots++;
                i += 3;
            }
            else
            {
                return false;
            }
        }

        return dots is 1 or 2;
    }

}
