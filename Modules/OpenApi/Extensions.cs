using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Discovery;
using GenHTTP.Modules.OpenApi.Handler;

using NSwag;

namespace GenHTTP.Modules.OpenApi;

public static class Extensions
{

    /// <summary>
    /// Adds a pre-configured concern to the given builder.
    /// </summary>
    /// <remarks>
    /// The generated concern will crawl through the inner handler chain and analyze the following
    /// types of content: Layouts, Concerns, Functional Handlers, Webservices, Controllers.
    /// </remarks>
    public static T AddOpenApi<T>(this T builder) where T : IHandlerBuilder<T>
        => builder.AddOpenApi(ApiDiscovery.Default());

    /// <summary>
    /// Creates a concern that will use the given discovery configuration to search for API endpoints
    /// to be added to the generated OpenAPI specification.
    /// </summary>
    /// <param name="registry">The explorer registry to be used to analyze the handler chain</param>
    public static T AddOpenApi<T>(this T builder, ApiDiscoveryRegistryBuilder registry) where T : IHandlerBuilder<T>
    {
        builder.Add(ApiDescription.With(registry));
        return builder;
    }

    /// <summary>
    /// Configures the generated OpenAPI specification to describe an
    /// API-key based authentication scheme.
    /// </summary>
    /// <remarks>
    /// This helper registers an <c>apiKey</c> security scheme that reads
    /// the key from a given HTTP header and marks all matching paths as
    /// requiring that scheme. A <c>401 Unauthorized</c> response is added
    /// to such operations if it is not already present.
    /// </remarks>
    /// <param name="builder">
    /// The OpenAPI concern builder returned by <see cref="ApiDescription.Create"/>.
    /// </param>
    /// <param name="schemeName">
    /// Logical name of the security scheme to be referenced by security requirements.
    /// Defaults to <c>"X-API-Key"</c>.
    /// </param>
    /// <param name="headerName">
    /// Name of the HTTP header that will contain the API key.
    /// Defaults to <c>"X-API-Key"</c>.
    /// </param>
    /// <param name="includePath">
    /// Optional predicate used to determine for which paths the security
    /// requirement should be applied. If <c>null</c>, the requirement is
    /// added for all paths.
    /// </param>
    /// <returns>
    /// The same <see cref="OpenApiConcernBuilder"/> instance to allow fluent configuration.
    /// </returns>
    public static OpenApiConcernBuilder WithApiKeyAuthentication(
    this OpenApiConcernBuilder builder,
    string schemeName = "X-API-Key",
    string headerName = "X-API-Key",
    Func<string, bool>? includePath = null)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        includePath ??= _ => true;

        return builder.PostProcessor((request, doc) =>
        {
            var securityDefinitions = doc.SecurityDefinitions;

            // Register the API key security scheme if not already present
            if (securityDefinitions != null && !securityDefinitions.ContainsKey(schemeName))
            {
                securityDefinitions.Add(schemeName, new OpenApiSecurityScheme
                {
                    Name = headerName,
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    In = OpenApiSecurityApiKeyLocation.Header,
                });
            }

            if (doc.Paths is null)
            {
                return;
            }

            var requirement = new OpenApiSecurityRequirement
            {
            { schemeName, Array.Empty<string>() }
            };

            foreach (var pathEntry in doc.Paths)
            {
                var path = pathEntry.Key;
                var pathItem = pathEntry.Value;

                if (!includePath(path) || pathItem is null)
                {
                    continue;
                }

                foreach (var operation in pathItem.Values)
                {
                    if (operation is null) continue;

                    operation.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Security.Add(requirement);

                    if (!operation.Responses.ContainsKey("401"))
                    {
                        operation.Responses.Add("401", new OpenApiResponse
                        {
                            Description = "Unauthorized"
                        });
                    }
                }
            }
        });
    }


}
