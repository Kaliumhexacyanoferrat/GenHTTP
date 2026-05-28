using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.IO;

namespace GenHTTP.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class SimpleBenchmark
{
    private readonly BenchmarkContext _context = new
    (
        "GET / HTTP/1.1\r\nHost: localhost:8080\r\n\r\n", 
        Content.From(Resource.FromString("Hello World")).Build()
    );

    [Benchmark]
    public ValueTask BenchmarkSimple() => _context.Execute();
    
}
