using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ErrorHandling.Provider;

public sealed class ErrorSentry<T> : IConcern where T : Exception
{

    #region Get-/Setters

    public IHandler Content { get; }

    private IErrorMapper<T> ErrorHandler { get; }

    #endregion

    #region Initialization

    public ErrorSentry(IHandler content, IErrorMapper<T> errorHandler)
    {
        Content = content;

        ErrorHandler = errorHandler;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        try
        {
            var responseTask = Content.HandleAsync(request);

            if (!responseTask.IsCompletedSuccessfully)
            {
                return HandleAsyncSlow(responseTask, request);
            }

            var response = responseTask.Result;

            if (response is null)
            {
                return ErrorHandler.GetNotFound(request, Content);
            }

            return new ValueTask<IResponse?>(response);
        }
        catch (T e)
        {
            return ErrorHandler.Map(request, Content, e);
        }
    }

    private async ValueTask<IResponse?> HandleAsyncSlow(ValueTask<IResponse?> pending, IRequest request)
    {
        try
        {
            var response = await pending;

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
