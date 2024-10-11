using System.Reflection;
using GenHTTP.Modules.IO.Embedded;
using GenHTTP.Modules.IO.FileSystem;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Provides a folder-like structure that can be used to generate responses.
/// </summary>
public static class ResourceTree
{

    /// <summary>
    /// Creates a resource tree that will provide all embedded
    /// resources provided by the currently executing assembly.
    /// </summary>
    public static EmbeddedResourceTreeBuilder FromAssembly()
    {
        var assembly = Assembly.GetCallingAssembly();

        var name = assembly.GetName().Name ?? throw new InvalidOperationException($"Unable to determine root namespace for assembly '{assembly}'");

        return FromAssembly(assembly, name);
    }

    /// <summary>
    /// Creates a resource tree that will provide all embedded
    /// resources provided by the given assembly.
    /// </summary>
    public static EmbeddedResourceTreeBuilder FromAssembly(Assembly source)
    {
        var name = source.GetName().Name ?? throw new InvalidOperationException($"Unable to determine root namespace for assembly '{source}'");

        return new EmbeddedResourceTreeBuilder().Source(source)
                                                .Root(name);
    }

    /// <summary>
    /// Creates a resource tree that will provide all embedded
    /// resources provided by the executing assembly and starting
    /// with the given prefix (e.g. "My.Namespace.Folder").
    /// </summary>
    public static EmbeddedResourceTreeBuilder FromAssembly(string root) => FromAssembly(Assembly.GetCallingAssembly(), root);

    /// <summary>
    /// Creates a resource tree that will provide all embedded
    /// resources provided by the specified assembly and starting
    /// with the given prefix (e.g. "My.Namespace.Folder").
    /// </summary>
    public static EmbeddedResourceTreeBuilder FromAssembly(Assembly source, string root) => new EmbeddedResourceTreeBuilder().Source(source).Root(root);

    /// <summary>
    /// Creates a resource tree from the given directory.
    /// </summary>
    /// <param name="directory">The full path of the directory to be provided</param>
    public static DirectoryTreeBuilder FromDirectory(string directory) => FromDirectory(new DirectoryInfo(directory));

    /// <summary>
    /// Creates a resource tree from the given directory.
    /// </summary>
    /// <param name="directory">The directory to be provided</param>
    public static DirectoryTreeBuilder FromDirectory(DirectoryInfo directory) => new DirectoryTreeBuilder().Directory(directory);
}
