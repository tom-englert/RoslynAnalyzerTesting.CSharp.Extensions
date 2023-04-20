﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Extensions;

/// <summary>
/// A test for a <see cref="DiagnosticSuppressor"/> that supports testing framework 
/// </summary>
/// <typeparam name="TAnalyzer">The analyzer that generates the diagnostics to test</typeparam>
/// <typeparam name="TSuppressor">The suppressor to test</typeparam>
/// <typeparam name="TVerifier">The verifier for <see cref="AnalyzerTest{TVerifier}"/></typeparam>
public class CSharpDiagnosticSuppressorTest<TAnalyzer, TSuppressor, TVerifier> : CSharpAnalyzerTest<TAnalyzer, TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TSuppressor : DiagnosticSuppressor, new()
    where TVerifier : IVerifier, new()
{
    /// <inheritdoc />
    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        return base.GetDiagnosticAnalyzers().Append(new TSuppressor());
    }

    /// <inheritdoc />
    protected override CompilationWithAnalyzers CreateCompilationWithAnalyzers(Compilation compilation,
        ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
    {
        // Workaround https://github.com/dotnet/roslyn-sdk/issues/1078
        TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
        return compilation.WithAnalyzers(analyzers,
            new CompilationWithAnalyzersOptions(options, null, true, false, true));
    }
}