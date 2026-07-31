using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Adapters.AspNetCore.Mapping;

public class SelfPreparingConcern(IHandler content) : IConcern
{
    private Lazy<Task>? _preparation;

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var preparation = LazyInitializer.EnsureInitialized(ref _preparation,
            () => new Lazy<Task>(() => content.PrepareAsync(request.Server).AsTask()));

        await preparation.Value.ConfigureAwait(false);

        return await content.HandleAsync(request).ConfigureAwait(false);
    }

}
