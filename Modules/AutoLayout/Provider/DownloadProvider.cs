using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.AutoLayout.Provider
{

    public class DownloadProvider : IResourceHandlerProvider
    {

        public bool Supports(IResource resource) => true;

        public ValueTask<IHandlerBuilder> GetHandlerAsync(IResource resource) => new(Content.From(resource));

    }

}
