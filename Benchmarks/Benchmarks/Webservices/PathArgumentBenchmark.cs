using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Benchmarks.Benchmarks.Webservices;

[MemoryDiagnoser]
public class PathArgumentBenchmark
{
    private BenchmarkContext _context = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        var request = "GET /42 HTTP/1.1\r\nHost: localhost:8080\r\n\r\n";

        _context = new BenchmarkContext(request, ServiceResource.From<PathArguments>().Build());

        await _context.PrepareAsync();
    }

    [Benchmark]
    public ValueTask BenchmarkPathArgument() => _context.Execute();

}
