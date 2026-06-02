using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;
using GenHTTP.Modules.Files;

namespace GenHTTP.Benchmarks.Benchmarks.IO;

[MemoryDiagnoser]
public class StaticFileBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkStaticFile() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var handler = Assets.From("./Resources");

        var request = "GET /file.js HTTP/1.1\r\nHost: localhost:8080\r\n\r\n";

        return new(request, handler.Build());
    }

}
