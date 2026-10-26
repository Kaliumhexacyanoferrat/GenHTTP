using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.Documentation;

/// <summary>
/// The documentation configured by the user on an authentication builder.
/// </summary>
internal sealed class SchemeDocumentation
{

    internal SecurityScheme? Scheme { get; set; }

    internal string? Description { get; set; }

    /// <summary>
    /// Determines the scheme to document a concern with.
    /// </summary>
    /// <param name="inferred">The scheme derived from the configuration of the concern</param>
    internal SecurityScheme Resolve(SecurityScheme inferred) => Describe(Scheme ?? inferred);

    /// <summary>
    /// Determines the scheme to document a concern with, if the concern
    /// is not always able to derive the scheme from its configuration.
    /// </summary>
    /// <param name="inferred">The scheme derived from the configuration of the concern, if known</param>
    /// <returns>The scheme to be documented or null, if the mechanism cannot be described</returns>
    internal SecurityScheme? ResolveOptional(SecurityScheme? inferred)
    {
        var scheme = Scheme ?? inferred;

        return scheme != null ? Describe(scheme) : null;
    }

    private SecurityScheme Describe(SecurityScheme scheme) => Description != null ? scheme with { Description = Description } : scheme;

}
