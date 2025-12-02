using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class EndPointCollection : List<IEndPoint>, IEndPointCollection;
