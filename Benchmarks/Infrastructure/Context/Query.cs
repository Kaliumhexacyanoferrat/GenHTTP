using GenHTTP.Api.Protocol;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class Query: Dictionary<string, string>, IRequestQuery;
