using Microsoft.CodeAnalysis;

namespace Sample;

[Generator(LanguageNames.CSharp)]
public class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(CreateOutput);
    }

    private void CreateOutput(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("SampleSource.g.cs", "// This is just a generated sample");
    }
}