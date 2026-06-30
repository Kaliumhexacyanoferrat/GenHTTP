using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities;

public sealed class AsyncFunctionalHandler : IHandler
{
    private readonly Func<IRequest, ValueTask<IResponse?>>? _responseProvider;

    #region Initialization

    public AsyncFunctionalHandler(Func<IRequest, ValueTask<IResponse?>>? responseProvider = null)
    {
        _responseProvider = responseProvider;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => _responseProvider?.Invoke(request) ?? default;

    #endregion

}
