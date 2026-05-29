using BenchmarkDotNet.Running;

using GenHTTP.Benchmarks.Benchmarks.Functional;

BenchmarkRunner.Run<JsonSerializationBenchmark>();

// await new JsonSerializationBenchmark().BenchmarkReturnedJson();