using System.Diagnostics.CodeAnalysis;
using GenHTTP.Modules.Webservices.Provider;

namespace GenHTTP.Modules.Webservices;

/// <summary>
/// Entry point to add webservice resources to another router.
/// </summary>
public static class ServiceResource
{

    /// <summary>
    /// Provides a router that will invoke the methods of the
    /// specified resource type to generate responses.
    /// </summary>
    /// <typeparam name="T">The resource type to be provided</typeparam>
    public static ServiceResourceBuilder From<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new() => new ServiceResourceBuilder().Type<T>();

    /// <summary>
    /// Provides a router that will invoke the methods of the
    /// specified resource instance to generate responses.
    /// </summary>
    /// <param name="instance">The instance to be provided</param>
    public static ServiceResourceBuilder From(object instance) => new ServiceResourceBuilder().Instance(instance);
}
