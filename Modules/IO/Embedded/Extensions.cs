using System;
using System.IO;
using System.Reflection;

namespace GenHTTP.Modules.IO.Embedded;

internal static class Extensions
{

    internal static DateTime GetModificationDate(this Assembly assembly)
    {
#pragma warning disable IL3000
            var location = assembly.Location;

            if (!string.IsNullOrEmpty(location))
            {
                var sourceFile = new FileInfo(location);

                return ((sourceFile.Exists) ? sourceFile.LastWriteTimeUtc : DateTime.UtcNow);
            }

            return DateTime.UtcNow;
#pragma warning restore IL3000
        }

}
