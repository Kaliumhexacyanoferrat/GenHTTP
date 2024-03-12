using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection.Injectors;
using GenHTTP.Modules.Webservices.Provider;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Extensions to simplify handling of service resources.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Adds the given webservice resource to the layout, accessible using
        /// the specified path.
        /// </summary>
        /// <typeparam name="T">The type of the resource to be added</typeparam>
        /// <param name="path">The path the resource should be available at</param>
        /// <param name="injectors">Optionally the injectors to be used by this service</param>
        /// <param name="formats">Optionally the formats to be used by this service</param>
        public static LayoutBuilder AddService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder layout, string path, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? formats = null) where T : new()
        {
            return layout.Add(path, ServiceResource.From<T>().Configured(injectors, formats));
        }

        /// <summary>
        /// Adds the given webservice resource to the layout, accessible using
        /// the specified path.
        /// </summary>
        /// <param name="path">The path the resource should be available at</param>
        /// <param name="instance">The webservice resource instance</param>
        /// <param name="injectors">Optionally the injectors to be used by this service</param>
        /// <param name="formats">Optionally the formats to be used by this service</param>
        public static LayoutBuilder AddService(this LayoutBuilder layout, string path, object instance, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? formats = null)
        {
            return layout.Add(path, ServiceResource.From(instance).Configured(injectors, formats));
        }

        private static ServiceResourceBuilder Configured(this ServiceResourceBuilder builder, IBuilder<InjectionRegistry>? injectors = null, IBuilder<SerializationRegistry>? formats = null)
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
