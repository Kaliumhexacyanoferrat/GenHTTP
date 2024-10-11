using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities;

public sealed class FunctionalHandler : IHandlerWithParent
{
    private readonly Func<IRequest, IResponse?>? _ResponseProvider;

    private IHandler? _Parent;

    #region Initialization

    public FunctionalHandler(Func<IRequest, IResponse?>? responseProvider = null)
    {
        _ResponseProvider = responseProvider;
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent
    {
        get => _Parent ?? throw new InvalidOperationException();
        set => _Parent = value;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => new(_ResponseProvider?.Invoke(request));

    #endregion

}
