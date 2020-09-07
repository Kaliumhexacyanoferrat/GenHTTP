using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{
    
    /// <summary>
    /// Handlers implementing this interface help to determine the path
    /// a handler is registered on by appending the segment they are
    /// responsible for to the resulting path.
    /// </summary>
    public interface IRootPathAppender
    {

        /// <summary>
        /// Appends the segment the handler is responsible for to
        /// the given path.
        /// </summary>
        /// <param name="path">The path the segment should be appended to</param>
        /// <param name="request">The currently handled request (if it somehow influences the path)</param>
        /// <param name="child">The child which caused this call</param>
        void Append(PathBuilder path, IRequest request, IHandler? child = null);

    }

}
