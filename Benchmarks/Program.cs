using BenchmarkDotNet.Running;

using GenHTTP.Benchmarks.Benchmarks.IO;

BenchmarkRunner.Run<StaticFileBenchmark>();

//await new StaticFileBenchmark().BenchmarkStaticFile();