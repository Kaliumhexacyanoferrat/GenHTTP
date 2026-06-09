using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.IO;

namespace GenHTTP.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class SimpleBenchmark
{
    private BenchmarkContext _context = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        _context = await CreateContext();
    }

    [Benchmark]
    public ValueTask BenchmarkSimple() => _context.Execute();

    private static async Task<BenchmarkContext> CreateContext()
    {
        var context = new BenchmarkContext
        (
            "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n",
            Content.From(Resource.FromString("Hello World")).Build()
        );

        await context.PrepareAsync();

        return context;
    }

}
