using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Infrastructure;

/// <summary>
/// Request handler which is installed by the engine as the root handler - all
/// requests will start processing from here on. Provides core functionality
/// such as rendering exceptions when they bubble up uncatched.
/// </summary>
internal sealed class CoreRouter : IHandler
{

    #region Initialization

    internal CoreRouter(IHandlerBuilder content, IEnumerable<IConcernBuilder> concerns)
    {
        Content = Concerns.Chain(this, concerns, content.Build);
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent
    {
        get => throw new NotSupportedException("Core router has no parent");
        set => throw new NotSupportedException("Setting core router's parent is not allowed");
    }

    internal IHandler Content { get; }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Content.HandleAsync(request);

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
