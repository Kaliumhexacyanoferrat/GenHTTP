using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Benchmarks.Benchmarks.Compression;

/// <summary>
/// Generated content (here: JSON) written through the sink's buffer writer
/// to a client that only accepts gzip.
/// </summary>
[MemoryDiagnoser]
public class GzipGeneratedResponseBenchmark
{
    private BenchmarkContext _context = default!;

    public record Item(int Id, string Name, string Description);

    [GlobalSetup]
    public async Task Setup()
    {
        _context = await CreateContext();
    }

    [Benchmark]
    public ValueTask BenchmarkGzipGeneratedResponse() => _context.Execute();

    private static async Task<BenchmarkContext> CreateContext()
    {
        var items = Enumerable.Range(0, 25)
                              .Select(i => new Item(i, $"Item {i}", "Some description of the item to give it a body"))
                              .ToArray();

        var handler = Inline.Create()
                            .Get(() => items)
                            .Add(CompressedContent.Default());

        var request = "GET / HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: gzip\r\n\r\n";

        var context = new BenchmarkContext(request, handler.Build());

        await context.PrepareAsync();

        return context;
    }

}
