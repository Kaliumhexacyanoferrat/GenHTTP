using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Interface which needs to be implemented by all models
    /// used to render pages or templates.
    /// </summary>
    public interface IModel
    {

        /// <summary>
        /// The request belonging to the current rendering call.
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// The handler responsible for the current rendering call.
        /// </summary>
        IHandler Handler { get; }

        /// <summary>
        /// Calculates the checksum of this model.
        /// </summary>
        /// <returns>The checksum of this model</returns>
        /// <remarks>
        /// Required to determine, whether an already cached page
        /// needs to be refreshed or not.
        /// </remarks>
        ValueTask<ulong> CalculateChecksumAsync();

    }

}
