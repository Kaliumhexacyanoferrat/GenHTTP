using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities;

public sealed class FunctionalHandler : IHandlerWithParent
{
    private readonly Func<IRequest, IResponse?>? _ResponseProvider;

    private IHandler? _Parent;

    #region Get-/Setters

    public IHandler Parent
    {
        get { return _Parent ?? throw new InvalidOperationException(); }
        set { _Parent = value; }
    }

    #endregion

    #region Initialization

    public FunctionalHandler(Func<IRequest, IResponse?>? responseProvider = null)
    {
            _ResponseProvider = responseProvider;
        }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => new((_ResponseProvider is not null) ? _ResponseProvider(request) : null);

    #endregion

}
