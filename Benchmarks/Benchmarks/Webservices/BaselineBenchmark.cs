using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Benchmarks.Benchmarks.Webservices;

[MemoryDiagnoser]
public class BaselineBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkBaseline() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var handler = ServiceResource.From<Baseline>();

        var request = "GET /?a=1&b=2 HTTP/1.1\r\nHost: localhost:8080\r\n\r\n";

        return new(request, handler.Build());
    }

}
