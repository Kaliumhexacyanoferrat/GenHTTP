namespace GenHTTP.Modules.Archives.Tree;

/// <summary>
/// As a resource tree needs to grant parallel access to all resources
/// and is not disposable, we need to open and seek the archive for
/// each file requested. This class ensures that the archive stream
/// is disposed as soon as the compressed file content is disposed.
/// </summary>
/// <param name="Handle">The archive handle to dispose at the end</param>
/// <param name="Content">The actual file content to be served</param>
internal record ArchiveHandle(IDisposable Handle, Stream Content);
