namespace GenHTTP.Modules.Archives.Tree;

internal record ArchiveHandle(IDisposable Handle, Stream Content);
