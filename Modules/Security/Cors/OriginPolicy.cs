using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Cors;

public record OriginPolicy(List<RequestMethod>? AllowedMethods, List<string>? AllowedHeaders,
    List<string>? ExposedHeaders, bool AllowCredentials, uint MaxAge);
