using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Files.Handlers;

public class TreeAssetsBuilder(IResourceTree tree) : IHandlerBuilder<TreeAssetsBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    public TreeAssetsBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new TreeAssetsHandler(tree));
    }

}
