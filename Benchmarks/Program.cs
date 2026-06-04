using GenHTTP.Benchmarks.Benchmarks.Webservices;

// BenchmarkRunner.Run<BaselineBenchmark>();

await new BaselineBodyBenchmark().BenchmarkBaseline();
