using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Kestrel.Hosting;

public sealed class KestrelEndpoints : List<IEndPoint>, IEndPointCollection;
