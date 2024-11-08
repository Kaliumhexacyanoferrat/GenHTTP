using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance;

[AttributeUsage(AttributeTargets.Method)]
public class MultiEngineTestAttribute : Attribute, ITestDataSource
{

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var engines = Environment.GetEnvironmentVariable("GENHTTP_TEST_ENGINES");

        if (engines != null)
        {
            var result = new List<object[]>();

            if (engines.Contains("Internal"))
            {
                result.Add(new object[] { TestEngine.Internal });
            }

            if (engines.Contains("Kestrel"))
            {
                result.Add(new object[] { TestEngine.Kestrel });
            }

            return result;
        }

        return new List<object[]>
        {
            new object[] { TestEngine.Internal}
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
