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
                SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass")
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
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass"));

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
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass"))
            .RunAsync();
    }

    private class CustomTest : CSharpAnalyzerTest<SampleAnalyzer, MSTestVerifier>
    {
        public CustomTest(string source)
        {
            TestCode = source;
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60.AddPackages(SampleNugetPackage);
            SolutionTransforms.Add(AddAssemblyReferences(typeof(Abstractions.SampleAttribute).Assembly));
            SolutionTransforms.Add(WithProjectCompilationOptions(options => options.WithCSharpDefaults()));
            SolutionTransforms.Add(WithLanguageVersion(LanguageVersion.CSharp10));
        }
    }

    [TestMethod]
    public async Task WithCustomTestClass()
    {
        await new CustomTest(Source)
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass"))
            .RunAsync();
    }

    [TestMethod]
    public async Task CodeFixTest()
    {
        var fixedSource = Source.Replace("SampleClass", "SAMPLECLASS", StringComparison.Ordinal);

        var test = new CSharpCodeFixTest<SampleAnalyzer, SampleCodeFixProvider, MSTestVerifier>()
            .AddSources(Source)
            .AddReferences(typeof(Abstractions.SampleAttribute).Assembly)
            .AddPackages(SampleNugetPackage)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults())
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass"))
            .AddFixedSources(fixedSource);

        await test.RunAsync();
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

    [TestMethod]
    public async Task SuppressorTest()
    {
        await new CSharpDiagnosticSuppressorTest<SampleAnalyzer, SampleSuppressor, MSTestVerifier>()
            .AddSources(Source)
            .AddReferences(typeof(Abstractions.SampleAttribute).Assembly)
            .AddPackages(SampleNugetPackage)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults())
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass").WithIsSuppressed(true))
            .RunAsync();
    }
}
