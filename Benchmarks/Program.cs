using BenchmarkDotNet.Running;
using GenHTTP.Benchmarks.Benchmarks.Webservices;

BenchmarkRunner.Run<BaselineBenchmark>();

/*var bench = new BaselineBenchmark();

await bench.Setup();
await bench.BenchmarkBaseline();*/
