using GenHTTP.Modules.IO.VirtualTrees;

namespace GenHTTP.Modules.IO;

public static class VirtualTree
{

    /// <summary>
    /// Creates a virtual tree that may contain any other kind
    /// of tree or resource and allows to combine them.
    /// </summary>
    public static VirtualTreeBuilder Create() => new();
}
