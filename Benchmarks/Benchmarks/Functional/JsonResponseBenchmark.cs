using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Functional;

namespace GenHTTP.Benchmarks.Benchmarks.Functional;

[MemoryDiagnoser]
public class JsonResponseBenchmark
{
    private BenchmarkContext _context = default!;

    [GlobalSetup]
    public async Task Setup()
    {
        var item = new SomeItem(Guid.NewGuid(), "Some string", 42, DateTime.Now);

        var handler = Inline.Create()
                            .Get(() => item);

        _context = new BenchmarkContext("GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n", handler.Build());

        await _context.PrepareAsync();
    }

    [Benchmark]
    public ValueTask BenchmarkJsonResponse() => _context.Execute();

}
