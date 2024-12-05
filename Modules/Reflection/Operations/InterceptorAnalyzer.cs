using System.Reflection;

namespace GenHTTP.Modules.Reflection.Operations;

public static class InterceptorAnalyzer
{

    public static IReadOnlyList<IOperationInterceptor> GetInterceptors(MethodInfo method)
    {
        var interceptors = new List<IOperationInterceptor>();

        foreach (var attribute in method.GetCustomAttributes(typeof(InterceptWithAttribute<>), true))
        {
            var interceptorType = FindInterceptorType(attribute.GetType());

            if (interceptorType != null)
            {
                if (Activator.CreateInstance(interceptorType) is IOperationInterceptor interceptor)
                {
                    interceptor.Configure(attribute);
                    interceptors.Add(interceptor);
                }
            }
        }

        return interceptors;
    }

    private static Type? FindInterceptorType(Type attributeType)
    {
        var current = attributeType;

        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(InterceptWithAttribute<>))
            {
                return current.GetGenericArguments()[0];
            }

            current = current.BaseType;
        }

        return null;
    }

}
