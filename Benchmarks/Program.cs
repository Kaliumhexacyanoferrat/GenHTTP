using BenchmarkDotNet.Running;

using GenHTTP.Benchmarks.Benchmarks.ServerCaching;

BenchmarkRunner.Run<PrecompressedBenchmark>();

// await new PrecompressedBenchmark().BenchmarkPrecompressed();