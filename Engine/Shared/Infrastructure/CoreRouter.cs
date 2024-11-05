using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Infrastructure;

/// <summary>
/// Request handler which is installed by the engine as the root handler - all
/// requests will start processing from here on. Provides core functionality
/// such as rendering exceptions when they bubble up uncatched.
/// </summary>
public sealed class CoreRouter : IHandler
{

    #region Get-/Setters

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public CoreRouter(IHandler content, IEnumerable<IConcernBuilder> concerns)
    {
        Content = Concerns.Chain(concerns, content);
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Content.HandleAsync(request);

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
