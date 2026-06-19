using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Static-file handler backed by ioxide.file's <see cref="StaticAssets"/> (an fd cache with baked
/// native responses + statx-based revalidation), instead of GenHTTP's <c>Modules.Files</c> - whose
/// <c>FileResource</c> asks the write target for a 64 KB buffer and overflows the ioxide write slab.
/// GenHTTP frames the status + headers; this module writes the body, flushing every &lt;= 12 KB so it
/// never stages more than a slab's worth. Mount with <c>Layout.Create().Add("static", IoxideFiles.From(dir))</c>.
/// </summary>
public static class IoxideFiles
{
    public static IoxideFilesBuilder From(string directory) => new(directory);
}
