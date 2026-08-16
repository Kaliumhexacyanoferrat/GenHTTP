using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.InternalH3Experimental.Infrastructure;

internal sealed class H3ServerHost : ServerHost
{
    private readonly int _qpackCapacity;

    internal H3ServerHost(int qpackCapacity) => _qpackCapacity = qpackCapacity;

    protected override IServer Build(ServerConfiguration config, IHandler handler)
        => new H3Server(config, handler, _qpackCapacity);

}
