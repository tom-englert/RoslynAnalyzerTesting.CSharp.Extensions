using Microsoft.CodeAnalysis;

namespace RoslynAnalyzerTestingExtensions.CSharp;

public class SourceGeneratorAdapter<TIncrementalGenerator> : ISourceGenerator, IIncrementalGenerator
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    private readonly TIncrementalGenerator _incrementalGenerator = new();
    private readonly ISourceGenerator _sourceGenerator;

    public SourceGeneratorAdapter()
    {
        _sourceGenerator = _incrementalGenerator.AsSourceGenerator();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _incrementalGenerator.Initialize(context);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        _sourceGenerator.Initialize(context);
    }

    public void Execute(GeneratorExecutionContext context)
    {
        _sourceGenerator.Execute(context);
    }
}