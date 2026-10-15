using System.Runtime.CompilerServices;

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
        var eagerRequired = request.HasBody;
        
        var eagerAccepted = (eagerRequired) ? request.Header.Headers.GetEntry(KnownHeaders.Accept) : null;

        try
        {
            var responseTask = Content.HandleAsync(request);

            if (!responseTask.IsCompletedSuccessfully)
            {
                return HandleAsyncSlow(responseTask, request, GetAccepted(request, eagerRequired, eagerAccepted));
            }

            var response = responseTask.Result;

            if (response is null)
            {
                return ErrorHandler.GetNotFound(request, Content, GetAccepted(request, eagerRequired, eagerAccepted));
            }

            return new ValueTask<IResponse?>(response);
        }
        catch (T e)
        {
            return ErrorHandler.Map(request, Content, e, GetAccepted(request, eagerRequired, eagerAccepted));
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ByteString? GetAccepted(IRequest request, bool eagerRequired, ByteString? eagerAccepted)
    {
        if (eagerRequired)
        {
            return eagerAccepted;
        }
        
        return request.Header.Headers.GetEntry(KnownHeaders.Accept);
    }

    #endregion

}
