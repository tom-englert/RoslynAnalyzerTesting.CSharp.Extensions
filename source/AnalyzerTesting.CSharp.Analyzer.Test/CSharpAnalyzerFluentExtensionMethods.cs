using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Analyzer.Test;

using Verifier = global::Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier;

public static class CSharpAnalyzerFluentExtensionMethods
{
    public static TTest AddPackages<TTest>(this TTest test, params PackageIdentity[] packages)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ReferenceAssemblies = test.ReferenceAssemblies.AddPackages(packages.ToImmutableArray());
        return test;
    }

    public static TTest AddDiagnostics<TTest>(this TTest test, params DiagnosticResult[] expected)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    public static TTest AddSolutionTransform<TTest>(this TTest test, Func<Solution, Project, Solution> transform)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId);
            return project == null ? solution : transform(solution, project);
        });

        return test;
    }

    public static TTest AddSources<TTest>(this TTest test, params string[] sources)
        where TTest : AnalyzerTest<Verifier>
    {
        foreach (var source in sources)
        {
            test.TestState.Sources.Add(source);
        }

        return test;
    }

    public static TTest AddReferences<TTest>(this TTest test, params Assembly[] localReferences)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var localMetadataReferences = localReferences
                .Distinct()
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

            solution = solution.AddMetadataReferences(projectId, localMetadataReferences);

            return solution;
        });

        return test;
    }

    public static TTest WithReferenceAssemblies<TTest>(this TTest test, ReferenceAssemblies referenceAssemblies)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ReferenceAssemblies = referenceAssemblies.AddPackages(test.ReferenceAssemblies.Packages);
        return test;
    }

    public static TTest WithLangVersion<TTest>(this TTest test, LanguageVersion languageVersion)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) => solution.WithProjectParseOptions(projectId, new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose)));
        return test;
    }

    public static TTest WithProjectCompilationOptions<TTest>(this TTest test, Func<CSharpCompilationOptions, CSharpCompilationOptions> callback)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId);
            var compilationOptions = (CSharpCompilationOptions?)project?.CompilationOptions;
            return compilationOptions == null ? solution : solution.WithProjectCompilationOptions(projectId, callback(compilationOptions));
        });
        return test;
    }
}