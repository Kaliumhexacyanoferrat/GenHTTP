using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;
using GenHTTP.Modules.Files;

namespace GenHTTP.Benchmarks.Benchmarks.IO;

[MemoryDiagnoser]
public class StaticFileBenchmark
{
    private BenchmarkContext _context = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        _context = await CreateContext();
    }

    [Benchmark]
    public ValueTask BenchmarkStaticFile() => _context.Execute();

    private static async Task<BenchmarkContext> CreateContext()
    {
        var handler = Assets.From("./Resources");

        var request = "GET /file.js HTTP/1.1\r\nHost: localhost:8080\r\n\r\n";

        var context = new BenchmarkContext(request, handler.Build());

        await context.PrepareAsync();

        return context;
    }

}
