using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ErrorHandling.Provider;

public sealed class ErrorSentry<T> : IConcern where T : Exception
{

    #region Get-/Setters

    public IHandler Content { get; }

    public IHandler Parent { get; }

    private IErrorMapper<T> ErrorHandler { get; }

    #endregion

    #region Initialization

    public ErrorSentry(IHandler parent, Func<IHandler, IHandler> contentFactory, IErrorMapper<T> errorHandler)
    {
            Parent = parent;
            Content = contentFactory(this);

            ErrorHandler = errorHandler;
        }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
            try
            {
                var response = await Content.HandleAsync(request);

                if (response is null)
                {
                    return await ErrorHandler.GetNotFound(request, Content);
                }

                return response;
            }
            catch (T e)
            {
                return await ErrorHandler.Map(request, Content, e);
            }
        }

    #endregion

}
