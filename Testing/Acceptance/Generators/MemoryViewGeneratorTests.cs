using System.Collections.Immutable;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using GenHTTP.Generators.MemoryView;

namespace GenHTTP.Testing.Acceptance.Generators;

[TestClass]
public sealed class MemoryViewGeneratorTests
{

    #region Supporting infrastructure

    private static (ImmutableArray<GeneratedSourceResult> Sources, ImmutableArray<Diagnostic> Diagnostics) Generate(string source)
    {
        var references = AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                                   .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName: "MemoryViewGeneratorTests.Generated",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new MemoryViewGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var result = driver.GetRunResult().Results.Single();

        return (result.GeneratedSources, outputCompilation.GetDiagnostics());
    }

    private static void AssertNoErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        Assert.IsTrue(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    #endregion

    #region Tests

    [TestMethod]
    public void TestStructInNamespaceIsGenerated()
    {
        const string source = """
            namespace MyApp;

            [GenHTTP.Api.MemoryViewAttribute]
            public readonly partial struct MyView { }
            """;

        var (sources, diagnostics) = Generate(source);

        Assert.AreEqual(1, sources.Length);
        Assert.AreEqual("MyApp.MyView.MemoryView.g.cs", sources[0].HintName);

        var text = sources[0].SourceText.ToString();

        StringAssert.Contains(text, "namespace MyApp;");
        StringAssert.Contains(text, "public readonly partial struct MyView");
        StringAssert.Contains(text, "public bool Equals(MyView other)");

        AssertNoErrors(diagnostics);
    }

    [TestMethod]
    public void TestStructInGlobalNamespaceIsGenerated()
    {
        const string source = """
            [GenHTTP.Api.MemoryViewAttribute]
            public readonly partial struct MyGlobalView { }
            """;

        var (sources, diagnostics) = Generate(source);

        Assert.AreEqual(1, sources.Length);
        Assert.AreEqual("MyGlobalView.MemoryView.g.cs", sources[0].HintName);

        StringAssert.DoesNotMatch(sources[0].SourceText.ToString(), new Regex("^namespace ", RegexOptions.Multiline));

        AssertNoErrors(diagnostics);
    }

    [TestMethod]
    public void TestStructWithoutAttributeIsIgnored()
    {
        const string source = """
            namespace MyApp;

            public readonly partial struct MyView { }
            """;

        var (sources, _) = Generate(source);

        Assert.AreEqual(0, sources.Length);
    }

    [TestMethod]
    public void TestMultipleStructsProduceIndependentSources()
    {
        const string source = """
            namespace MyApp;

            [GenHTTP.Api.MemoryViewAttribute]
            public readonly partial struct First { }

            [GenHTTP.Api.MemoryViewAttribute]
            public readonly partial struct Second { }
            """;

        var (sources, diagnostics) = Generate(source);

        Assert.AreEqual(2, sources.Length);

        CollectionAssert.AreEquivalent(
            new[] { "MyApp.First.MemoryView.g.cs", "MyApp.Second.MemoryView.g.cs" },
            sources.Select(s => s.HintName).ToList()
        );

        AssertNoErrors(diagnostics);
    }

    #endregion

}
