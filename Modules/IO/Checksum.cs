using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO;

public static class Checksum
{

    public static ulong Calculate(IResource resource)
    {
        unchecked
        {
            ulong hash = 17;

            var length = resource.Length;

            hash = hash * 23 + (ulong)resource.Modified.GetHashCode();
            hash = hash * 23 + (length ?? 0);

            return hash;
        }
    }

}
