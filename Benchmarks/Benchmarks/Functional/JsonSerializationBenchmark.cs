using BenchmarkDotNet.Attributes;

using GenHTTP.Benchmarks.Infrastructure;

using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Benchmarks.Benchmarks.Functional;

public record SomeItem(Guid Id, String StringValue, int IntValue, DateTime DateValue);

[MemoryDiagnoser]
public class JsonSerializationBenchmark
{
    private readonly BenchmarkContext _context = CreateContext();

    [Benchmark]
    public ValueTask BenchmarkReturnedJson() => _context.Execute();

    private static BenchmarkContext CreateContext()
    {
        var list = new List<SomeItem>();

        var random = new Random();
        
        for (var i = 0; i < 500; i++)
        {
            list.Add(new(Guid.NewGuid(), "Some string", random.Next(), DateTime.Now));
        }
        
        var handler = Inline.Create()
                            .Get(() => list)
                            .Add(CompressedContent.Default());

        var request = "GET / HTTP/1.1\r\nHost: localhost:8080\r\nAccept-Encoding: br;q=1, gzip;q=0.8\r\n\r\n";

        return new(request, handler.Build());
    }

}
