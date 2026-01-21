using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO.Tracking;

namespace GenHTTP.Modules.Archives.Tree;

public class ArchivedTreeBuilder(ChangeTrackingResource source) : IBuilder<IResourceTree>
{

    public IResourceTree Build()
    {
        return new ArchivedTree(source);
    }

}
