using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Benchmarks.Benchmarks.Webservices;

[MemoryDiagnoser]
public class BaselineBodyBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkBaseline() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var handler = ServiceResource.From<Baseline>();

        var request = "POST /?a=1&b=2 HTTP/1.1\r\nHost: localhost:8080\r\nContent-Length: 2\r\n\r\n20";

        return new(request, handler.Build());
    }

}
