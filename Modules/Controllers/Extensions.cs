using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Controllers.Provider;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection.Injectors;

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
        /// <param name="injectors">Optionally the injectors to be used by this controller</param>
        /// <param name="formats">Optionally the formats to be used by this controller</param>
        public static LayoutBuilder AddController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder builder, string path, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? formats = null) where T : new()
        {
            builder.Add(path, Controller.From<T>().Configured(injectors, formats));
            return builder;
        }

        /// <summary>
        /// Causes the specified controller class to be used to handle the index of 
        /// this layout.
        /// </summary>
        /// <typeparam name="T">The type of the controller used to handle requests</typeparam>
        /// <param name="builder">The layout the controller should be added to</param>
        /// <param name="injectors">Optionally the injectors to be used by this controller</param>
        /// <param name="formats">Optionally the formats to be used by this controller</param>
        public static LayoutBuilder IndexController<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder builder, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? formats = null) where T : new()
        {
            builder.Add(Controller.From<T>().Configured(injectors, formats));
            return builder;
        }

        private static ControllerBuilder<T> Configured<T>(this ControllerBuilder<T> builder, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? formats = null) where T : new()
        {
            if (injectors != null)
            {
                builder.Injectors(injectors); 
            }

            if (formats != null)
            {
                builder.Serializers(formats);
            }

            return builder;
        }

    }

}
