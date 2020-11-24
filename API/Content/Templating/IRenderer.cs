using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Allows to render models of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the model to be rendered</typeparam>
    public interface IRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] in T> where T : class, IBaseModel
    {

        /// <summary>
        /// Asynchronously initializes the renderer so
        /// computation heavy work does not need to be
        /// done on first rendering request.
        /// </summary>
        ValueTask PrepareAsync();

        /// <summary>
        /// Renders the given model.
        /// </summary>
        /// <param name="model">The model to be rendered</param>
        /// <returns>The rendered model</returns>
        ValueTask<string> RenderAsync(T model);

    }

}
