using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Adapters.AspNetCore.Server;

public class EmptyEndpoints : List<IEndPoint>, IEndPointCollection;
