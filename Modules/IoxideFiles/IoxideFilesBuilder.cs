using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using ioxide.file;

namespace GenHTTP.Modules.IoxideFiles;

/// <summary>
/// Builds an <see cref="IoxideFilesHandler"/> over a shared <see cref="StaticAssets"/> cache and lets
/// concerns be chained onto it. Created via <see cref="IoxideFiles.From(string)"/>.
/// </summary>
public sealed class IoxideFilesBuilder : IHandlerBuilder<IoxideFilesBuilder>
{
    private readonly StaticAssets _assets;

    private readonly List<IConcernBuilder> _concerns = [];

    internal IoxideFilesBuilder(string directory)
    {
        // Opened once and shared across reactors (fds are stable, reads positional); the per-reactor
        // AssetReader pool is resolved lazily in the content.
        _assets = new StaticAssets(directory);
    }

    public IoxideFilesBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, new IoxideFilesHandler(_assets));
}
