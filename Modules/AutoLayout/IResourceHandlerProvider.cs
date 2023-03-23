using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.AutoLayout
{

    public interface IResourceHandlerProvider
    {

        public bool Supports(IResource resource);

        ValueTask<IHandlerBuilder> GetHandlerAsync(IResource resource);

    }

}
