using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Extensions;

public class CSharpIncrementalSourceGeneratorTest<TSourceGenerator, TVerifier> 
    : CSharpSourceGeneratorTest<SourceGeneratorAdapter<TSourceGenerator>, TVerifier>
    where TSourceGenerator : IIncrementalGenerator, new()
    where TVerifier : IVerifier, new()
{
}