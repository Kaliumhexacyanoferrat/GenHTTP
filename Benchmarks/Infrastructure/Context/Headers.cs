using GenHTTP.Api.Protocol;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class Headers : Dictionary<string, string>, IHeaderCollection;
