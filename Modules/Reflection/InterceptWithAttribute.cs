namespace GenHTTP.Modules.Reflection;

/// <summary>
/// When annotated on a service method, the method handler
/// will create an instance of T and invoke it before
/// the actual method invocation.
/// </summary>
/// <typeparam name="T">The type of interceptor to be used</typeparam>
/// <remarks>
/// Allows to implement concerns on operation level such as authorization.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class InterceptWithAttribute<T> : Attribute where T : IOperationInterceptor, new()
{

}
