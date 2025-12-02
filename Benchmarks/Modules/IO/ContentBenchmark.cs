using BenchmarkDotNet.Attributes;
using GenHTTP.Benchmarks.Infrastructure;
using GenHTTP.Benchmarks.Infrastructure.Context;
using GenHTTP.Modules.IO;

namespace GenHTTP.Benchmarks.Modules.IO;

[MemoryDiagnoser]
public class ContentBenchmark : HandlerBenchmark
{

    public ContentBenchmark()
    {
        Handler = Content.From(Resource.FromString("Hello World!")).Build();
        Context = BenchmarkContext.Create();
    }

    [Benchmark]
    public ValueTask BenchmarkContent() => Run(true);

    [Benchmark]
    public ValueTask BenchmarkContentHandlerOnly() => Run(false);

}
