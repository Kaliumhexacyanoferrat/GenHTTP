using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
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

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var accepted = request.Header.Headers.GetEntry(KnownHeaders.Accept);

        try
        {
            var responseTask = Content.HandleAsync(request);

            if (!responseTask.IsCompletedSuccessfully)
            {
                return HandleAsyncSlow(responseTask, request, accepted);
            }

            var response = responseTask.Result;

            if (response is null)
            {
                return ErrorHandler.GetNotFound(request, Content, accepted);
            }

            return new ValueTask<IResponse?>(response);
        }
        catch (T e)
        {
            return ErrorHandler.Map(request, Content, e, accepted);
        }
    }

    private async ValueTask<IResponse?> HandleAsyncSlow(ValueTask<IResponse?> pending, IRequest request, ByteString? accepted)
    {
        try
        {
            var response = await pending;

            if (response is null)
            {
                return await ErrorHandler.GetNotFound(request, Content, accepted);
            }

            return response;
        }
        catch (T e)
        {
            return await ErrorHandler.Map(request, Content, e, accepted);
        }
    }

    #endregion

}
