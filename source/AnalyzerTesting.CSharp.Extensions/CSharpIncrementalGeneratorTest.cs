using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace AnalyzerTesting.CSharp.Extensions;

/// <summary>
/// A <see cref="CSharpSourceGeneratorTest{TSourceGenerator,TVerifier}"/> for incremental source generators.
/// </summary>
/// <typeparam name="TSourceGenerator"></typeparam>
/// <typeparam name="TVerifier"></typeparam>
public class CSharpIncrementalGeneratorTest<TSourceGenerator, TVerifier> 
    : CSharpSourceGeneratorTest<SourceGeneratorAdapter<TSourceGenerator>, TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TVerifier : IVerifier, new()
{
    /// <summary>
    /// Adds the specified source to the TestState.GeneratedSources collection.
    /// </summary>
#pragma warning disable CA1044 // Properties should not be write only
    public (string fileName, string sourceCode) GeneratedSource
    {
        set
        {
            var generatedCode = SourceText.From(value.sourceCode, Encoding.UTF8);
            var generatorType = typeof(SourceGeneratorAdapter<TSourceGenerator>);
            var generatedFile = $@"{generatorType.Namespace}\{generatorType.FullName}\{value.fileName}";
            var generatedSource = (generatedFile, generatedCode);

            TestState.GeneratedSources.Add(generatedSource);
        }
    }
}
