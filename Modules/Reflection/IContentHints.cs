using System;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection
{

    /// <summary>
    /// Contains all variables that are needed to determine
    /// the target path of the content being discovered.
    /// </summary>
    [Serializable]
    public class ContentHint : Dictionary<string, object?>
    {

        public ContentHint() : base() { }

    }

    /// <summary>
    /// Implementations of this interface can be used with the
    /// <see cref="MethodAttribute"/> to discover the content
    /// provided by a service method. 
    /// </summary>
    /// <remarks>
    /// Implementations must (at least) return the required
    /// input parameters to determine the path of the target URL.
    /// Query parameters are supported, yet optional.
    /// </remarks>
    public interface IContentHints
    {

        /// <summary>
        /// Fetches the hints to discover the content of the
        /// service method. Every item the returned collection
        /// will represent one content URL derived from the
        /// variables.
        /// </summary>
        /// <param name="request">The request which actually led to content generation</param>
        /// <returns>The hints of the content to be discovered</returns>
        List<ContentHint> GetHints(IRequest request);

    }

}
