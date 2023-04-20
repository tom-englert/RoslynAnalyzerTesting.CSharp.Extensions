using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Extensions;

/// <summary>
/// A source generator test that does not internally verify the generated sources, but returns them as a string so you can use it with snapshot testing.
/// </summary>
/// <typeparam name="TSourceGenerator">The source generator to test.</typeparam>
/// <typeparam name="TVerifier">The verifier of the <see cref="AnalyzerTest{TVerifier}"/></typeparam>
public class CSharpIncrementalGeneratorSnapshotTest<TSourceGenerator, TVerifier> : CSharpIncrementalGeneratorTest<TSourceGenerator, TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TVerifier : IVerifier, new()
{
    private string? _generatedSources;

    /// <summary>
    /// Creates an instance of the <see cref="CSharpIncrementalGeneratorSnapshotTest{TSourceGenerator,TVerifier}"/>
    /// </summary>
    public CSharpIncrementalGeneratorSnapshotTest()
    {
        TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck | TestBehaviors.SkipSuppressionCheck | TestBehaviors.SkipGeneratedSourcesCheck;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public new async Task<string> RunAsync(CancellationToken cancellationToken = default)
    {
        await base.RunAsync(cancellationToken);

        return _generatedSources ?? string.Empty;
    }

    /// <inheritdoc />
    protected override async Task<Compilation> GetProjectCompilationAsync(Project project, IVerifier verifier,
        CancellationToken cancellationToken)
    {
        var compilation = await base.GetProjectCompilationAsync(project, verifier, cancellationToken);

        _generatedSources = JoinResults(compilation.SyntaxTrees.Skip(project.DocumentIds.Count));

        return compilation;
    }

    private static string JoinResults(IEnumerable<SyntaxTree> results)
    {
        return string.Join("\r\n",
            results.Select(result => $"// {Path.GetFileName(result.FilePath)}\r\n{result.GetText()}"));
    }
}
