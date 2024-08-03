using System.Threading.Tasks;
using AnalyzerTesting.CSharp.Extensions;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyMSTest;

namespace AnalyzerTesting.CSharp.Analyzer.Test;

using GeneratorTest = CSharpIncrementalGeneratorSnapshotTest<SourceGenerator, DefaultVerifier>;

[TestClass]
public class ExtensionMethodsCodeGeneratorUnitTest : VerifyBase
{
    private static readonly PackageIdentity TestPackage = new("Microsoft.CodeAnalysis.CSharp.Analyzer.Testing", "1.1.2");

    private static GeneratorTest BuildTest(params string[] sources)
    {
        return new GeneratorTest()
            .AddSources(sources)
            .WithReferenceAssemblies(ReferenceAssemblies.NetStandard.NetStandard20)
            .AddPackages(
                new PackageIdentity("Microsoft.CodeAnalysis.CSharp.CodeFix.Testing", "1.1.2"),
                new PackageIdentity("Microsoft.CodeAnalysis.CSharp", "4.3.1"),
                new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Workspaces", "4.3.1")
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
            .AddPackages(TestPackage)
            .RunAsync();

        await Verify(generated);
    }

    [TestMethod]
    public async Task TestWithOnlyAnalyzerSourceDoesNotGenerateAnything()
    {
        var generated = await BuildTest(AnalyzerSource)
            .AddPackages(TestPackage)
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
        
        using Verifier = Microsoft.CodeAnalysis.Testing.DefaultVerifier;
        
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
            .AddPackages(TestPackage)
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
        
        public class Test 
        {
            public async Task AnyTest()
            {
                await new CSharpAnalyzerTest<MyAnalyzer, DefaultVerifier>().RunAsync();
            }
        }
        """;

        var generated = await BuildTest(AnalyzerSource, testSource)
            .AddPackages(TestPackage)
            .RunAsync();

        await Verify(generated);
    }
}
