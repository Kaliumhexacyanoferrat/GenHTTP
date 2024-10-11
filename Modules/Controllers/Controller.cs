using System.Diagnostics.CodeAnalysis;

using GenHTTP.Modules.Controllers.Provider;

namespace GenHTTP.Modules.Controllers;

public static class Controller
{

    /// <summary>
    /// Creates a handler that will use the given controller class to generate responses.
    /// </summary>
    /// <typeparam name="T">The type of the controller to be used</typeparam>
    /// <returns>The newly created request handler</returns>
    public static ControllerBuilder From<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new() => new ControllerBuilder().Type<T>();

    /// <summary>
    /// Creates a handler that will use the given controller instance to generate responses.
    /// </summary>
    /// <param name="instance">The instance to be used</param>
    /// <returns>The newly created request handler</returns>
    public static ControllerBuilder From(object instance) => new ControllerBuilder().Instance(instance);

}
