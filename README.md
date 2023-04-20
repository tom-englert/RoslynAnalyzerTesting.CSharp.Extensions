# Roslyn analyzer testing extensions

### Instantly start coding roslyn analyzer tests, without having to prepare a new derived class for every test flavor.

This project provides solution transforms and extension methods to simpify testing of roslyn analyzers 
and to find how to configure common compilation tasks like adding references or changing language version.

It also provides base classes to test incremental generators and diagnostic suppressors with `"Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` version `1.1.1` and `Microsoft.CodeAnalysis.CSharp.Workspaces` >= `4.3.0`


- Predefined solution transforms to fine tune the test when using object initializer notation
- Extension methods to configure the test using fluent notation
- An `IncrementalSourceGenerator` test to test incremental source generators with `"Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` version `1.1.1`
- An `IncrementalSourceGenerator` test that supports snapshot testing
- A `DiagnosticSuppressor` test to test diagnostic suppressors with `Microsoft.CodeAnalysis.CSharp.Workspaces` >= `4.3.0`

## Sample code demonstrating the usage

<!-- snippet: Test.cs -->
<a id='snippet-Test.cs'></a>
```cs
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sample;

[TestClass]
public class Test
{
    private static readonly PackageIdentity SampleNugetPackage = new("TomsToolbox.Essentials", "2.8.7");

    private const string Source = """
                // Attribute introduced by an assembly reference
                [Sample.Abstractions.SampleAttribute]
                // Attribute introduced by a NuGet package
                [TomsToolbox.Essentials.TextAttribute("Key", "Value")]
                class {|#0:SampleClass|} 
                { 
                }
                """;

    [TestMethod]
    public async Task WithObjectInitializer()
    {
        var test = new CSharpAnalyzerTest<SampleAnalyzer, MSTestVerifier>
        {
            TestCode = Source,
            SolutionTransforms =
            {
                AddAssemblyReferences(typeof(Sample.Abstractions.SampleAttribute).Assembly),
                WithProjectCompilationOptions(options => options.WithCSharpDefaults()),
                WithLanguageVersion(LanguageVersion.CSharp10)
            },
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60.AddPackages(SampleNugetPackage),
            ExpectedDiagnostics =
            {
                SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("MessageArg")
            }
        };

        await test.RunAsync();
    }

    [TestMethod]
    public async Task WithFluentExtensions()
    {
        var test = new CSharpAnalyzerTest<SampleAnalyzer, MSTestVerifier>()
            .AddSources(Source)
            .AddReferences(typeof(Abstractions.SampleAttribute).Assembly)
            .AddPackages(SampleNugetPackage)
            .WithLanguageVersion(LanguageVersion.CSharp10)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults())
            .WithReferenceAssemblies(ReferenceAssemblies.Net.Net60)
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("MessageArg"));

        await test.RunAsync();
    }

    private static CSharpAnalyzerTest<SampleAnalyzer, MSTestVerifier> BuildTest(params string[] sources)
    {
        return new CSharpAnalyzerTest<SampleAnalyzer, MSTestVerifier>()
            .AddSources(sources)
            .AddReferences(typeof(Abstractions.SampleAttribute).Assembly)
            .AddPackages(SampleNugetPackage)
            .WithLanguageVersion(LanguageVersion.CSharp10)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults())
            .WithReferenceAssemblies(ReferenceAssemblies.Net.Net60);
    }

    [TestMethod]
    public async Task WithSharedTestBuilder()
    {
        await BuildTest(Source)
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("MessageArg"))
            .RunAsync();
    }

    [TestMethod]
    public async Task CodeGeneratorSnapshotTest()
    {
        var test = new CSharpIncrementalGeneratorSnapshotTest<SampleGenerator, MSTestVerifier>()
            .AddSources(Source)
            .AddReferences(typeof(Abstractions.SampleAttribute).Assembly)
            .AddPackages(SampleNugetPackage)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults());

        var generated = await test.RunAsync();

        // Replace this with your favorite snapshot testing framework, e.g. Verify
        Assert.AreEqual("// SampleSource.g.cs\r\n// This is just a generated sample", generated);
    }
}
```
<sup><a href='/source/Sample/Sample/Test.cs#L1-L94' title='Snippet source file'>snippet source</a> | <a href='#snippet-Test.cs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
