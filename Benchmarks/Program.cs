using BenchmarkDotNet.Running;

using GenHTTP.Benchmarks.Benchmarks.Compression;

BenchmarkRunner.Run<PreCompressedBenchmark>();

// await new PreCompressedBenchmark().BenchmarkPreCompressedStaticFiles();