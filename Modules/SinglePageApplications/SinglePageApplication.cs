using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.SinglePageApplications.Provider;

namespace GenHTTP.Modules.SinglePageApplications;

public static class SinglePageApplication
{

    public static SinglePageBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

    public static SinglePageBuilder From(IResourceTree tree) => new SinglePageBuilder().Tree(tree);

}