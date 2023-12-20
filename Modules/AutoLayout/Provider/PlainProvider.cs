using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Placeholders;

namespace GenHTTP.Modules.AutoLayout.Provider
{

    public class PlainProvider : IResourceHandlerProvider
    {

        public bool Supports(IResource resource)
        {
            var type = (resource.ContentType?.KnownType ?? resource.Name?.GuessContentType());
            
            return (type == ContentType.TextPlain) || (type == ContentType.TextHtml);
        }

        public ValueTask<IHandlerBuilder> GetHandlerAsync(IResource resource)
        {
            return new(Page.From(resource));
        }

    }

}
