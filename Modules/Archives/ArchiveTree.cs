using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.Archives.Tree;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Tracking;

namespace GenHTTP.Modules.Archives;

public static class ArchiveTree
{

    public static ArchivedTreeBuilder From<T>(IResourceBuilder<T> source) where T : IResourceBuilder<T> => From(source.BuildWithTracking());

    public static ArchivedTreeBuilder From(IResource source) => From(new(source));

    public static ArchivedTreeBuilder From(ChangeTrackingResource source) => new(source);

}
