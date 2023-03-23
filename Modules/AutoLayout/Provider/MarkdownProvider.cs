using System;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Markdown;

namespace GenHTTP.Modules.AutoLayout.Provider
{

    public class MarkdownProvider : IResourceHandlerProvider
    {

        public bool Supports(IResource resource) => (resource.ContentType?.KnownType ?? resource.Name?.GuessContentType()) == ContentType.TextMarkdown;

        public ValueTask<IHandlerBuilder> GetHandlerAsync(IResource resource)
        {
            return new(ModMarkdown.Page(resource));
        }

    }

}
