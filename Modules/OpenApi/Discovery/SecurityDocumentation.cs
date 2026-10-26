using GenHTTP.Api.Content.Documentation.Security;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

/// <summary>
/// Converts the authentication requirements declared by concerns and
/// interceptors into security schemes and requirements of a document.
/// </summary>
internal static class SecurityDocumentation
{

    #region Requirements

    /// <summary>
    /// Expands the given requirements (which all need to be satisfied) into the
    /// alternatives expected by the OpenAPI specification and registers the referenced
    /// schemes with the document.
    /// </summary>
    internal static List<OpenApiSecurityRequirement> GetRequirements(OpenApiDocument document, IEnumerable<AuthenticationRequirement> requirements)
    {
        List<HashSet<string>> alternatives = [[]];

        foreach (var requirement in requirements)
        {
            var names = requirement.AnyOf.Select(s => Register(document, s)).Distinct().ToList();

            alternatives = alternatives.SelectMany(a => names.Select(n => new HashSet<string>(a) { n })).ToList();
        }

        return ToRequirements(alternatives);
    }

    /// <summary>
    /// Creates the alternatives that satisfy both of the given ones.
    /// </summary>
    internal static List<OpenApiSecurityRequirement> Combine(IEnumerable<OpenApiSecurityRequirement> first, IEnumerable<OpenApiSecurityRequirement> second)
    {
        var alternatives = first.SelectMany(a => second.Select(b => new HashSet<string>(a.Keys.Concat(b.Keys)))).ToList();

        return ToRequirements(alternatives);
    }

    private static List<OpenApiSecurityRequirement> ToRequirements(List<HashSet<string>> alternatives)
    {
        var result = new List<OpenApiSecurityRequirement>();

        foreach (var alternative in alternatives)
        {
            if (result.Any(r => alternative.SetEquals(r.Keys)))
            {
                continue;
            }

            var requirement = new OpenApiSecurityRequirement();

            foreach (var name in alternative.Order(StringComparer.Ordinal))
            {
                requirement.Add(name, []);
            }

            result.Add(requirement);
        }

        return result;
    }

    #endregion

    #region Schemes

    /// <summary>
    /// Adds the given scheme to the document, if not already present.
    /// </summary>
    /// <returns>The name the scheme can be referenced with</returns>
    /// <remarks>
    /// If the name of the scheme is already used by a different scheme,
    /// a number will be appended to the name.
    /// </remarks>
    internal static string Register(OpenApiDocument document, SecurityScheme scheme)
    {
        var converted = Convert(scheme);

        var schemes = document.Components.SecuritySchemes;

        for (var i = 1; ; i++)
        {
            var name = (i == 1) ? scheme.Name : $"{scheme.Name}{i}";

            if (!schemes.TryGetValue(name, out var existing))
            {
                schemes.Add(name, converted);
                return name;
            }

            if (AreEqual(existing, converted))
            {
                return name;
            }
        }
    }

    private static OpenApiSecurityScheme Convert(SecurityScheme scheme) => scheme switch
    {
        ApiKeySecurityScheme apiKey => new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = apiKey.Parameter,
            In = apiKey.Location switch
            {
                ApiKeyLocation.Query => OpenApiSecurityApiKeyLocation.Query,
                ApiKeyLocation.Cookie => OpenApiSecurityApiKeyLocation.Cookie,
                _ => OpenApiSecurityApiKeyLocation.Header
            },
            Description = apiKey.Description
        },
        HttpSecurityScheme http => new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.Http,
            Scheme = http.Scheme,
            BearerFormat = http.BearerFormat,
            Description = http.Description
        },
        OpenIdConnectSecurityScheme openId => new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.OpenIdConnect,
            OpenIdConnectUrl = openId.DiscoveryUrl,
            Description = openId.Description
        },
        _ => throw new NotSupportedException($"Security scheme '{scheme.GetType()}' is not supported")
    };

    private static bool AreEqual(OpenApiSecurityScheme x, OpenApiSecurityScheme y)
        => x.Type == y.Type
           && x.Name == y.Name
           && x.In == y.In
           && x.Scheme == y.Scheme
           && x.BearerFormat == y.BearerFormat
           && x.OpenIdConnectUrl == y.OpenIdConnectUrl
           && x.Description == y.Description;

    #endregion

}
