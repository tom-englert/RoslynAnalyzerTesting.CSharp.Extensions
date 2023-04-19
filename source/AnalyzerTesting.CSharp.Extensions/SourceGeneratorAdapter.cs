using Microsoft.CodeAnalysis;

namespace AnalyzerTesting.CSharp.Extensions;

/// <summary>
/// Adapter to test an IIncrementalGenerator with the CSharpSourceGeneratorTest
/// </summary>
/// <typeparam name="TIncrementalGenerator">The incremental generator to test</typeparam>
public class SourceGeneratorAdapter<TIncrementalGenerator> : ISourceGenerator, IIncrementalGenerator
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    private readonly TIncrementalGenerator _incrementalGenerator = new();
    private readonly ISourceGenerator _sourceGenerator;

    /// <summary>
    /// Creates a new <see cref="SourceGeneratorAdapter{TIncrementalGenerator}"/>
    /// </summary>
    public SourceGeneratorAdapter()
    {
        _sourceGenerator = _incrementalGenerator.AsSourceGenerator();
    }

    /// <inheritdoc />>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _incrementalGenerator.Initialize(context);
    }

    /// <inheritdoc />>
    public void Initialize(GeneratorInitializationContext context)
    {
        _sourceGenerator.Initialize(context);
    }

    /// <inheritdoc />>
    public void Execute(GeneratorExecutionContext context)
    {
        _sourceGenerator.Execute(context);
    }
}