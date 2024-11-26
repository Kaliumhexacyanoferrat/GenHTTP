using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance;

/// <summary>
/// When attributed on a test method, this attribute
/// injects the engines that are target to be tested
/// into the test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MultiEngineTestAttribute : Attribute, ITestDataSource
{

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        return new List<object[]>
        {
            new object[] { TestEngine.Internal },
            new object[] { TestEngine.Kestrel }
        };
    }

    public string GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        if (data?[0] is TestEngine engine)
        {
            return $"{methodInfo.Name} ({engine.ToString()})";
        }

        return methodInfo.Name;
    }

}
