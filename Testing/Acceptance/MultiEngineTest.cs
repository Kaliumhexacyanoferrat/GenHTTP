using System.Reflection;

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
        var engine = Environment.GetEnvironmentVariable("TEST_ENGINE");
        
        if (engine == null) {
            return new List<object[]>
            {
                new object[] { TestEngine.Internal },
                new object[] { TestEngine.Kestrel }
            };
        }

        if (Enum.TryParse(engine, out TestEngine found))
        {
            return new List<object[]>
            {
                new object[] { found }
            };
        }

        throw new InvalidOperationException($"Engine '{engine}' is not supported");
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
