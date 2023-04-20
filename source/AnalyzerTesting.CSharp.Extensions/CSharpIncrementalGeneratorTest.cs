using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

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
}