namespace GenHTTP.Api.Modules.Templating
{

    /// <summary>
    /// Allows to render models of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the model to be rendered</typeparam>
    public interface IRenderer<in T> where T : IBaseModel
    {

        /// <summary>
        /// Renders the given model.
        /// </summary>
        /// <param name="model">The model to be rendered</param>
        /// <returns>The rendered model</returns>
        string Render(T model);

    }

}
