namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Marking an argument of a service method with this attribute will
/// cause the parameter to be read from the body of the request.
/// </summary>
/// <remarks>
/// This attribute can be used on all parameters with simple types
/// (such as string or int). Complex types will always be deserialized
/// from the body without the need of marking it explicitly.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromBodyAttribute : Attribute
{

}
