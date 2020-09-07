using GenHTTP.Modules.Webservices.Provider;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Entry point to add webservice resources to another router.
    /// </summary>
    public static class Resource
    {

        /// <summary>
        /// Provides a router that will invoke the methods of the
        /// specified resource type to generate responses.
        /// </summary>
        /// <typeparam name="T">The resource type to be provided</typeparam>
        public static ResourceBuilder From<T>() where T : new() => new ResourceBuilder().Type<T>();

        /// <summary>
        /// Provides a router that will invoke the methods of the
        /// specified resource instance to generate responses.
        /// </summary>
        /// <param name="instance">The instance to be provided</param>
        public static ResourceBuilder From(object instance) => new ResourceBuilder().Instance(instance);

    }

}
