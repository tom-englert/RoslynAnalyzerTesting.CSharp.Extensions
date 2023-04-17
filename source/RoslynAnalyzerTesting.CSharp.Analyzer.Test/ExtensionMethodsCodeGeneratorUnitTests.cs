using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynAnalyzerTestingExtensions.CSharp;
using VerifyMSTest;

namespace ExtensionMethodsCodeGenerator.Test;

using GeneratorTest = SourceGeneratorTest<SourceGeneratorAdapter<SourceGenerator>>;

[TestClass]
public class ExtensionMethodsCodeGeneratorUnitTest : VerifyBase
{
    private static readonly PackageIdentity MsTestPackage = new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.MSTest", "1.1.1");
    private static readonly PackageIdentity XUnitPackage = new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit", "1.1.1");

    const string AnalyzerSource = """
            using System.Collections.Immutable;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.Diagnostics;
            
            public class MyAnalyzer : DiagnosticAnalyzer
            {
                public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray<DiagnosticDescriptor>.Empty;
                public override void Initialize(AnalysisContext context)
                {
                }
            }
            """;

    [TestMethod]
    public async Task EmptySourceDoesNotGenerateAnything()
    {
        const string source = "";

        var generated = await new GeneratorTest(source)
            .AddPackages(MsTestPackage)
            .RunAsync();

        await Verify(generated);
    }

    [TestMethod]
    public async Task TestWithOnlyAnalyzerSourceDoesNotGenerateAnything()
    {
        var generated = await new GeneratorTest(AnalyzerSource)
            .AddPackages(MsTestPackage)
            .RunAsync();

        await Verify(generated);
    }

    [TestMethod]
    public async Task TestWithMsTestVerifierAndExplicitClass()
    {
        const string testSource = """
        using System.Collections.Immutable;
        using System.Threading.Tasks;

        using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp.Testing;
        using Microsoft.CodeAnalysis.Diagnostics;
        using Microsoft.CodeAnalysis.Testing;
        
        using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier;
        
        public class AnalyzerTest : CSharpAnalyzerTest<MyAnalyzer, Verifier>
        {
        }

        public class Test 
        {
            public async Task AnyTest()
            {
                await new AnalyzerTest().RunAsync();
            }
        }
        """;

        var generated = await new GeneratorTest(AnalyzerSource, testSource)
            .AddPackages(MsTestPackage)
            .RunAsync();

        await Verify(generated);
    }

    [TestMethod]
    public async Task TestWithXUnitVerifierAndAdHocClass()
    {
        const string testSource = """
        using System.Collections.Immutable;
        using System.Threading.Tasks;

        using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp.Testing;
        using Microsoft.CodeAnalysis.Diagnostics;
        using Microsoft.CodeAnalysis.Testing;
        using Microsoft.CodeAnalysis.Testing.Verifiers;
        
        public class Test 
        {
            public async Task AnyTest()
            {
                await new CSharpAnalyzerTest<MyAnalyzer, XUnitVerifier>().RunAsync();
            }
        }
        """;

        var generated = await new GeneratorTest(AnalyzerSource, testSource)
            .AddPackages(XUnitPackage)
            .RunAsync();

        await Verify(generated);
    }
}