using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyMSTest;

namespace AnalyzerTesting.CSharp.Analyzer.Test;

using GeneratorTest = CSharpIncrementalGeneratorSnapshotTest<SourceGenerator, MSTestVerifier>;

[TestClass]
public class ExtensionMethodsCodeGeneratorUnitTest : VerifyBase
{
    private static readonly PackageIdentity MsTestPackage = new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.MSTest", "1.1.1");
    private static readonly PackageIdentity XUnitPackage = new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit", "1.1.1");

    private static GeneratorTest BuildTest(params string[] sources)
    {
        return new GeneratorTest()
            .AddSources(sources)
            .WithReferenceAssemblies(ReferenceAssemblies.NetStandard.NetStandard20)
            .AddPackages(
                new PackageIdentity("Microsoft.CodeAnalysis.CSharp", "4.3.0"),
                new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Workspaces", "4.3.0")
            );
    }

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

        var generated = await BuildTest(source)
            .AddPackages(MsTestPackage)
            .RunAsync();

        await Verify(generated);
    }

    [TestMethod]
    public async Task TestWithOnlyAnalyzerSourceDoesNotGenerateAnything()
    {
        var generated = await BuildTest(AnalyzerSource)
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

        var generated = await BuildTest(AnalyzerSource, testSource)
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

        var generated = await BuildTest(AnalyzerSource, testSource)
            .AddPackages(XUnitPackage)
            .RunAsync();

        await Verify(generated);
    }
}