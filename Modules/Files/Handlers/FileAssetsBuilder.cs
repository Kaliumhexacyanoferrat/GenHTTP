using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Files.Handlers;

public sealed class FileAssetsBuilder(DirectoryInfo directory) : IHandlerBuilder<FileAssetsBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    public FileAssetsBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new FileAssetsHandler(directory));
    }

}
