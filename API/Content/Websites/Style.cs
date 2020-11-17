using GenHTTP.Api.Content.IO;

namespace GenHTTP.Api.Content.Websites
{

    /// <summary>
    /// A stylesheet that can be used within a website.
    /// </summary>
    public record Style(string Name, IResource Provider);

}
