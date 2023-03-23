using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Scriban;

namespace GenHTTP.Modules.AutoLayout.Provider
{

    public class ScribanProvider : IResourceHandlerProvider
    {

        public bool Supports(IResource resource) => (resource.ContentType?.KnownType ?? resource.Name?.GuessContentType()) == ContentType.TextScriban;

        public ValueTask<IHandlerBuilder> GetHandlerAsync(IResource resource)
        {
            return new(ModScriban.Page(resource));
        }

    }

}
