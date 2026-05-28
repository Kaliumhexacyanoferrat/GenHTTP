using BenchmarkDotNet.Running;

using GenHTTP.Benchmarks.Benchmarks.Compression;

BenchmarkRunner.Run<CompressedResponseBenchmark>();

// await new CompressedResponseBenchmark().BenchmarkCompressedResponse();