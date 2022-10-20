using System;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Can be used with the error handling module to generate custom
    /// responses for exceptions thrown during request handling.
    /// </summary>
    /// <typeparam name="T">The type of exception to be mapped (others will not be handled)</typeparam>
    public interface IErrorMapper<in T> where T : Exception
    {

        /// <summary>
        /// Generates a HTTP response to be sent for the given exception.
        /// </summary>
        /// <param name="request">The request which caused the error</param>
        /// <param name="handler">The handler which catched the exception</param>
        /// <param name="error">The actual exception to be mapped</param>
        /// <returns>A HTTP response to be sent or null, if the error should be handled as not found by the next error handler in the chain</returns>
        ValueTask<IResponse?> Map(IRequest request, IHandler handler, T error);

        /// <summary>
        /// Generates a HTTP response for a resource that has not been found.
        /// </summary>
        /// <param name="request">The currently handled request</param>
        /// <param name="handler">The inner  handler of the error handling concern</param>
        /// <returns>A HTTP response to be sent or null, if the error should be handled as not found by the next error handler in the chain</returns>
        ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler);

    }

}
