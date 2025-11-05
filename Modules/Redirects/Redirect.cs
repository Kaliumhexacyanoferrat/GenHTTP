using GenHTTP.Modules.Redirects.Provider;

namespace GenHTTP.Modules.Redirects;

public static class Redirect
{

    /// <summary>
    /// Redirects the requesting client to the specified location.
    /// </summary>
    /// <param name="location">The location to redirect the client to (an absolute or relative route)</param>
    /// <param name="temporary"><c>false</c>, if the client should remember this redirection</param>
    /// <example>
    ///     <code>
    /// Redirect.To("https://genhttp.org", true);
    /// Redirect.To("some/other/site", false);
    /// Redirect.To("{sitemap}", true);
    /// </code>
    /// </example>
    public static RedirectProviderBuilder To(string location, bool temporary = false) => new RedirectProviderBuilder().Location(location)
                                                                                                                      .Mode(temporary);
}
