using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Benchmarks.Benchmarks.Compression;

/// <summary>
/// Generated content (here: JSON) compressed with the algorithm
/// the client asks for.
/// </summary>
[MemoryDiagnoser]
public class CompressionAlgorithmBenchmark
{
    private BenchmarkContext _context = default!;

    public record Item(int Id, string Name, string Description);

    [Params("br", "zstd", "gzip")]
    public string Algorithm { get; set; } = "br";

    [GlobalSetup]
    public async Task Setup()
    {
        var items = Enumerable.Range(0, 25)
                              .Select(i => new Item(i, $"Item {i}", "Some description of the item to give it a body"))
                              .ToArray();

        var handler = Inline.Create()
                            .Get(() => items)
                            .Add(CompressedContent.Default());

        var request = $"GET / HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: {Algorithm}\r\n\r\n";

        _context = new BenchmarkContext(request, handler.Build());

        await _context.PrepareAsync();
    }

    [Benchmark]
    public ValueTask BenchmarkCompressedResponse() => _context.Execute();

}
