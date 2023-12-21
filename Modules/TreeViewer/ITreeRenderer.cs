using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using System.Threading.Tasks;

namespace GenHTTP.Modules.TreeViewer
{

    public record class TreeViewItem(string Path, IHandlerBuilder Handler);

    public interface ITreeRenderer
    {

        ValueTask<IHandlerBuilder?> GetIndexAsync(IResourceContainer container);

        ValueTask<TreeViewItem?> GetHandlerAsync(IResourceContainer container, IResource resource);

    }

}
