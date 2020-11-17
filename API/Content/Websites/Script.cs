using GenHTTP.Api.Content.IO;

namespace GenHTTP.Api.Content.Websites
{

    /// <summary>
    /// A script file that can be used within a website.
    /// </summary>
    public record Script(string Name, bool Async, IResource Provider);

}
