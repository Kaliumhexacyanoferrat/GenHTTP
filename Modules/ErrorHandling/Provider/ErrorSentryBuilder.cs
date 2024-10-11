using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ErrorHandling.Provider;

public sealed class ErrorSentryBuilder<T> : IConcernBuilder where T : Exception
{

    #region Initialization

    public ErrorSentryBuilder(IErrorMapper<T> handler)
    {
        Handler = handler;
    }

    #endregion

    #region Get-/Setters

    private IErrorMapper<T> Handler { get; }

    #endregion

    #region Functionality

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory) => new ErrorSentry<T>(parent, contentFactory, Handler);

    #endregion

}
