using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Sample;

[TestClass]
public class Test : VerifyBase
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

    private sealed class CustomTest : CSharpAnalyzerTest<SampleAnalyzer, MSTestVerifier>
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
    public async Task CodeGeneratorTest()
    {
        var test = new CSharpIncrementalGeneratorTest<SampleGenerator, MSTestVerifier>()
        {
            GeneratedSource = ("SampleSource.g.cs", "// This is just a generated sample")
        };

        await test.RunAsync();
    }

    [TestMethod]
    public async Task CodeGeneratorSnapshotTest()
    {
        var test = new CSharpIncrementalGeneratorSnapshotTest<SampleGenerator, MSTestVerifier>
        {
            TestCode = "namespace Dummy { }"
        };

        var generated = await test.RunAsync();

        // Replace this with your favorite snapshot testing framework, e.g. https://github.com/VerifyTests/Verify
        await Verify(generated);
    }

    [TestMethod]
    public async Task AnalyzerSuppressorTest()
    {
        await new CSharpDiagnosticSuppressorTest<SampleAnalyzer, SampleSuppressor, MSTestVerifier>()
            .AddSources(Source)
            .AddReferences(typeof(Abstractions.SampleAttribute).Assembly)
            .AddPackages(SampleNugetPackage)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults())
            .AddExpectedDiagnostics(SampleAnalyzer.SampleDiagnostic.AsResult().WithLocation(0).WithArguments("SampleClass").WithIsSuppressed(true))
            .RunAsync();
    }

    [TestMethod]
    public async Task CompilerWarningSuppressorTest()
    {
        const string source = """
                              class C
                              {
                                  object x;
                                  
                                  {|#0:C|}() {}
                              }
                              """;

        await new CSharpDiagnosticSuppressorTest<SampleSuppressor, MSTestVerifier>()
            .AddSources(source)
            .WithProjectCompilationOptions(options => options.WithCSharpDefaults())
            .AddExpectedDiagnostics(DiagnosticResult.CompilerError("CS8618").WithLocation(0).WithArguments("field", "x").WithIsSuppressed(true))
            .RunAsync();
    }

}
