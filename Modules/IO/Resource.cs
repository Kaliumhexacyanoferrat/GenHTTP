using System.Reflection;

using GenHTTP.Modules.IO.Embedded;
using GenHTTP.Modules.IO.FileSystem;
using GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Allows to build resource instances which can be used by other
/// handlers to generate their content.
/// </summary>
public static class Resource
{

    /// <summary>
    /// Generates a resource from the given string.
    /// </summary>
    /// <param name="data">The string that will be returned by the resource (UTF-8-encoded)</param>
    public static StringResourceBuilder FromString(string data) => new StringResourceBuilder().Content(data);

    /// <summary>
    /// Searches within the current assembly for an embedded resource
    /// with the given name and generates a resource from it.
    /// </summary>
    /// <param name="name">The name of the resource to search for (may be fully qualified or not, e.g. "File.txt")</param>
    public static EmbeddedResourceBuilder FromAssembly(string name) => new EmbeddedResourceBuilder().Path(name).Assembly(Assembly.GetCallingAssembly());

    /// <summary>
    /// Searches within the given assembly for an embedded resource
    /// with the specified name and generates a resource from it.
    /// </summary>
    /// <param name="assembly">The assembly to search the embedded resource in</param>
    /// <param name="name">The name of the resource to search for (may be fully qualified or not, e.g. "File.txt")</param>
    public static EmbeddedResourceBuilder FromAssembly(Assembly assembly, string name) => new EmbeddedResourceBuilder().Assembly(assembly).Path(name);

    /// <summary>
    /// Generates a resource from the given file.
    /// </summary>
    /// <param name="file">The path of the file to be provided</param>
    public static FileResourceBuilder FromFile(string file) => FromFile(new FileInfo(file));

    /// <summary>
    /// Generates a resource from the given file.
    /// </summary>
    /// <param name="file">The file to be provided</param>
    public static FileResourceBuilder FromFile(FileInfo file) => new FileResourceBuilder().File(file);

}
