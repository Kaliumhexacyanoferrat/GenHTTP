using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// A test that only runs where the ioxide engine can: on Linux, which is the only platform its
/// io_uring backend exists for. On Windows and macOS the test is reported inconclusive rather than
/// starting a server that cannot run there.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IoxideTestMethodAttribute([CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
    : TestMethodAttribute(callerFilePath, callerLineNumber)
{
    
    private static readonly bool Supported = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public override Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        var engine = Environment.GetEnvironmentVariable("TEST_ENGINE");

        var engineAllowsIoxide = (engine is null) || string.Compare(engine, "ioxide", StringComparison.OrdinalIgnoreCase) == 0;
        
        if (!Supported || !engineAllowsIoxide)
        {
            return Task.FromResult<TestResult[]>(
            [
                new TestResult
                {
                    Outcome = UnitTestOutcome.Inconclusive,
                    TestFailureException = new AssertInconclusiveException("The ioxide engine is Linux-only (io_uring); skipped on this platform."),
                },
            ]);
        }

        return base.ExecuteAsync(testMethod);
    }
    
}
