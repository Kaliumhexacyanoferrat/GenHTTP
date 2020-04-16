using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    public class ErrorModel : PageModel
    {

        #region Get-/Setters

        public ResponseStatus Status { get; }

        public bool DevelopmentMode { get; }

        public string Message { get; }

        public Exception? Cause { get; }

        #endregion

        #region Initialization

        public ErrorModel(IRequest request, IHandler handler, ResponseStatus status, string title, string message, Exception? cause) : base(request, handler)
        {
            Status = status;
            DevelopmentMode = request.Server.Development;

            Title = title;
            Message = message;

            Cause = cause;
        }

        #endregion

    }

}
