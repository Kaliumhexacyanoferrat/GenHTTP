using System.Reflection;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance;

/// <summary>
/// When attributed on a test method, this attribute
/// injects the both the engine as well the execution
/// mode to be used.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MultiEngineFrameworkTestAttribute : Attribute, ITestDataSource
{

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var engines = new MultiEngineTestAttribute().GetData(methodInfo); // todo: ugly

        var result = new List<object[]>();

        foreach (var engine in engines)
        {
            result.Add([engine[0], ExecutionMode.Reflection]);
            result.Add([engine[0], ExecutionMode.Auto]);
        }

        return result;
    }

    public string GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        if (data?.Length == 2)
        {
            return $"{methodInfo.Name} ({data[0]}, {data[1]})";
        }

        return methodInfo.Name;
    }

}
