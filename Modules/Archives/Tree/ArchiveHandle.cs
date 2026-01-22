namespace GenHTTP.Modules.Archives.Tree;

public record ArchiveHandle(IDisposable Handle, Stream Content);
