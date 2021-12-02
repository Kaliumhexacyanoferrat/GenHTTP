using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Allows to render models of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the model to be rendered</typeparam>
    public interface IRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] in T> where T : class, IModel
    {

        /// <summary>
        /// Asynchronously initializes the renderer so
        /// computation heavy work does not need to be
        /// done on first rendering request.
        /// </summary>
        ValueTask PrepareAsync();

        /// <summary>
        /// Returns a checksum for the template used by this renderer instance.
        /// </summary>
        /// <returns>The checksum of the template used by this renderer</returns>
        ValueTask<ulong> CalculateChecksumAsync();

        /// <summary>
        /// Renders the given model.
        /// </summary>
        /// <param name="model">The model to be rendered</param>
        /// <returns>The rendered model</returns>
        ValueTask<string> RenderAsync(T model);

        /// <summary>
        /// Renders the given model into the specified stream.
        /// </summary>
        /// <param name="model">The model to be rendered</param>
        /// <param name="target">The stream to write to</param>
        ValueTask RenderAsync(T model, Stream target);

    }

}
