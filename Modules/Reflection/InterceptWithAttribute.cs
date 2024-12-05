namespace GenHTTP.Modules.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class InterceptWithAttribute<T> : Attribute where T : IOperationInterceptor, new()
{

}
