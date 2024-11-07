using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance;

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
