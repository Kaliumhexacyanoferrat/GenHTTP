using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Model for errors that occur when handling requests.
    /// </summary>
    public class ErrorModel : PageModel
    {

        #region Get-/Setters

        /// <summary>
        /// The status to be returned to the requesting client.
        /// </summary>
        public ResponseStatus Status { get; }

        /// <summary>
        /// Specifies, whether the server is in development mode.
        /// </summary>
        public bool DevelopmentMode { get; }

        /// <summary>
        /// The error message to be rendered.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The cause of this error, if any.
        /// </summary>
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
