using GenHTTP.Modules.Layouting.Provider;

using System.Diagnostics.CodeAnalysis;

namespace GenHTTP.Modules.Controllers
{

    public static class Extensions
    {

        /// <summary>
        /// Causes all requests to the specified path to be handled by the
        /// given controller class.
        /// </summary>
        /// <typeparam name="T">The type of the controller used to handle requests</typeparam>
        /// <param name="builder">The layout the controller should be added to</param>
        /// <param name="path">The path that should be handled by the controller</param>
        public static LayoutBuilder AddController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder builder, string path) where T : new()
        {
            builder.Add(path, Controller.From<T>());
            return builder;
        }

        /// <summary>
        /// Causes the specified controller class to be used to handle the index of 
        /// this layout.
        /// </summary>
        /// <typeparam name="T">The type of the controller used to handle requests</typeparam>
        /// <param name="builder">The layout the controller should be added to</param>
        public static LayoutBuilder IndexController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder builder) where T : new()
        {
            builder.Add(Controller.From<T>());
            return builder;
        }

    }

}
