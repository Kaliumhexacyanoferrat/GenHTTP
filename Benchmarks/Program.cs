using BenchmarkDotNet.Running;

using GenHTTP.Benchmarks.Benchmarks;

BenchmarkRunner.Run<SimpleBenchmark>();

// await new SimpleBenchmark().BenchmarkSimple();