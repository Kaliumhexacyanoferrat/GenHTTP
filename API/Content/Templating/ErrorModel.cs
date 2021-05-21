using System;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Model for errors that occur when handling requests.
    /// </summary>
    public sealed class ErrorModel : AbstractModel
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

        public ErrorModel(IRequest request, IHandler handler, ResponseStatus status, string message, Exception? cause) : base(request, handler)
        {
            Status = status;
            DevelopmentMode = request.Server.Development;

            Message = message;
            Cause = cause;
        }

        #endregion

        #region Functionality

        public override ValueTask<ulong> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + (uint)Status;

                hash = hash * 23 + (uint)Message.GetHashCode();

                hash = hash * 23 + (uint)(DevelopmentMode ? 1 : 0);
                hash = hash * 23 + (uint)(Cause?.GetHashCode() ?? 0);

                return new(hash);
            }
        }

        #endregion

    }

}
