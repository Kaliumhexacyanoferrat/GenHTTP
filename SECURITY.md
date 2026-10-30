# Security Policy

GenHTTP handles requests from untrusted clients, so we take security issues seriously and appreciate every
report that helps to keep applications built with GenHTTP safe.

## Reporting a Vulnerability

Please report vulnerabilities privately via email to **solutions@genhttp.org**.

Do not open a public issue, pull request or discussion for a suspected vulnerability, and do not mention it
in commit messages. Public disclosure before a fix is available puts every application using GenHTTP at risk.

Your report should contain:

- The affected module(s) or engine(s) and the GenHTTP version you tested with
- A description of the issue and its impact (e.g. information disclosure, denial of service)
- The steps or the request(s) needed to reproduce it
- Any known workarounds or a suggested fix, if you have one

## Proving Reproducibility

The best way to report an issue is an acceptance test that demonstrates it. A test removes any doubt about
the conditions needed to trigger the issue. It runs against all server engines, and after the fix it stays in
the test suite as a regression test.

Write the test the way the existing tests in [`Testing/Acceptance`](./Testing/Acceptance) are written. Configure
the handler as an application would, send the request an attacker would send, and assert the *secure*
behavior, so that the test fails as long as the issue exists:

```csharp
[TestClass]
public sealed class MyIssueTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIssueCannotBeExploited(ServerEngine engine)
    {
        // configure the handler the way an application would use it
        var app = Layout.Create()
                        .Add("api", ...);

        await using var host = await TestHost.RunAsync(app, engine: engine);

        // send the malicious request
        using var response = await host.GetResponseAsync("/api/...");

        // assert the secure behavior - fails while the issue exists
        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

}
```

You can run your test locally with:

```bash
dotnet test Testing/Acceptance/GenHTTP.Testing.Acceptance.csproj --filter "FullyQualifiedName~MyIssueTests"
```

Please attach the test to your email (as a file or patch) instead of pushing it to a public fork or branch.
If a test is not feasible (e.g. because the issue crashes the test host), describe the reproduction steps
or include a minimal program instead.

## What Happens Next

1. We confirm that we received your report.
2. We verify the issue, assess its impact and keep you informed about the progress.
3. We develop a fix, including a regression test, and release it.
4. After the release, the issue is disclosed publicly. You are credited unless you prefer to stay anonymous.

If you would like to contribute the fix yourself, please let us know in your report so we can coordinate
how and when it is published.

## Supported Versions

Security fixes are provided for the latest release of the current major version. Please verify that the
issue can still be reproduced with the latest release or the `main` branch before reporting.
