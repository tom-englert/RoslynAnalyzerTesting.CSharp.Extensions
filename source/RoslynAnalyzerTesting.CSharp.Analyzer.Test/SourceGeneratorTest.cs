using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using RoslynAnalyzerTesting.CSharp.Extensions;

namespace RoslynAnalyzerTesting.CSharp.Analyzer.Test;

public class SourceGeneratorTest<TSourceGenerator> : CSharpIncrementalSourceGeneratorTest<TSourceGenerator, MSTestVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    private string? _generatedSources;

    public SourceGeneratorTest(params string[] sources)
    {
        foreach (var source in sources)
        {
            TestState.Sources.Add(source);
        }
        TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck | TestBehaviors.SkipSuppressionCheck | TestBehaviors.SkipGeneratedSourcesCheck;
        ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard20.AddPackages(ImmutableArray.Create(
            new PackageIdentity("Microsoft.CodeAnalysis.CSharp", "4.3.0"),
            new PackageIdentity("Microsoft.CodeAnalysis.CSharp.Workspaces", "4.3.0")
        ));
    }

    public new async Task<string> RunAsync(CancellationToken cancellationToken = default)
    {
        await base.RunAsync(cancellationToken);

        return _generatedSources ?? string.Empty;
    }

    protected override async Task<Compilation> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
    {
        var compilation = await base.GetProjectCompilationAsync(project, verifier, cancellationToken);

        _generatedSources = JoinResults(compilation.SyntaxTrees.Skip(project.DocumentIds.Count));

        return compilation;
    }

    protected override CompilationOptions CreateCompilationOptions() => new CSharpCompilationOptions(
        OutputKind.DynamicallyLinkedLibrary,
        allowUnsafe: true,
        nullableContextOptions: NullableContextOptions.Enable);

    static string JoinResults(IEnumerable<SyntaxTree> results)
    {
        return string.Join("\r\n", results.Select(result => $"// {Path.GetFileName(result.FilePath)}\r\n{result.GetText()}"));
    }
}