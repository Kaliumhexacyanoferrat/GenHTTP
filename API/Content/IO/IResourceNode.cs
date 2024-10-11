namespace GenHTTP.Api.Content.IO;

/// <summary>
/// Provides a single hierarchy level in a structure
/// provided by a resource tree.
/// </summary>
public interface IResourceNode : IResourceContainer
{

    /// <summary>
    /// The name of this node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The parent of this node.
    /// </summary>
    IResourceContainer Parent { get; }
}
