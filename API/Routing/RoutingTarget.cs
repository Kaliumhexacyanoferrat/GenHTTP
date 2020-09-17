using System;
using System.Linq;

namespace GenHTTP.Api.Routing
{
    
    /// <summary>
    /// Provides a view on the target path of a request.
    /// </summary>
    /// <remarks>
    /// Stores the state of the routing mechanism and allows handlers to
    /// get the remaining parts to be handled.
    /// </remarks>
    public class RoutingTarget
    {
        private int _Index = 0;

        #region Get-/Setters

        /// <summary>
        /// The path of the request to be handled by the server.
        /// </summary>
        public WebPath Path { get; }

        /// <summary>
        /// The segment to be currently handled by the responsible handler.
        /// </summary>
        public string? Current => (_Index < Path.Parts.Count) ? Path.Parts[_Index] : null;

        /// <summary>
        /// Specifies, whether the end of the path has been reached.
        /// </summary>
        public bool Ended => (_Index >= Path.Parts.Count);

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new routing target and sets the pointer to the beginning of the path.
        /// </summary>
        /// <param name="path">The targeted path</param>
        public RoutingTarget(WebPath path)
        {
            Path = path;
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Acknowledges the currently handled segment and advances the
        /// pointer to the next one.
        /// </summary>
        public void Advance()
        {
            if (Ended)
            {
                throw new InvalidOperationException("Already at the end of the path");
            }
            
            _Index++;
        }

        /// <summary>
        /// Retrieves the part of the path that still needs to be routed.
        /// </summary>
        /// <returns>The remaining part of the path</returns>
        public WebPath GetRemaining() => new WebPath(Path.Parts.Skip(_Index).ToList(), Path.TrailingSlash);

        #endregion

    }

}
