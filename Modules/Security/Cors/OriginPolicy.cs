using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Cors;

public record OriginPolicy(List<FlexibleRequestMethod>? AllowedMethods, List<string>? AllowedHeaders,
    List<string>? ExposedHeaders, bool AllowCredentials, uint MaxAge);
