using System.Reflection;
using System.Runtime.InteropServices;

namespace GenHTTP.Testing.Acceptance;

/// <summary>
/// When attributed on a test method, this attribute
/// injects the engines that are target to be tested
/// into the test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MultiEngineTestAttribute : Attribute, ITestDataSource
{
    
#if NET11_0_OR_GREATER
    private static readonly bool IoxideSupported = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
    private static readonly bool IoxideSupported = false;
#endif

    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var engine = Environment.GetEnvironmentVariable("TEST_ENGINE");

        if (engine == null || engine == "CI") {
            var engines = new List<object[]>
            {
                new object[] { TestEngine.Internal },
                // todo: new object[] { TestEngine.Kestrel }
            };

            if (IoxideSupported && engine != "CI")
            {
                engines.Add(new object[] { TestEngine.Ioxide });
            }

            return engines;
        }

        if (Enum.TryParse(engine, out TestEngine found))
        {
            if (found == TestEngine.Ioxide && !IoxideSupported)
            {
                throw new InvalidOperationException("The ioxide engine is Linux-only and cannot be tested on this platform");
            }

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
