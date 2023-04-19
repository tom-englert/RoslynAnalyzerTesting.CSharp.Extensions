﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Analyzer.Test;

using Verifier = global::Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier;

internal static class CSharpAnalyzerFluentExtensions
{
    /// <summary>
    /// Adds nuget package references to the test compilation
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="packages">The packages to reference</param>
    /// <returns>The test</returns>
    public static TTest AddPackages<TTest>(this TTest test, params PackageIdentity[] packages)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ReferenceAssemblies = test.ReferenceAssemblies.AddPackages(packages.ToImmutableArray());
        return test;
    }

    /// <summary>
    /// Adds the diagnostics to the list of expected diagnostics
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="expected">The diagnostics</param>
    /// <returns>The test</returns>
    public static TTest AddExpectedDiagnostics<TTest>(this TTest test, params DiagnosticResult[] expected)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    /// <summary>
    /// Adds a solution transform to the test compilation
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="transform">The transforms to add</param>
    /// <returns>The test</returns>
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

    /// <summary>
    /// Adds source files to the test compilation
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="sources">The sources to add</param>
    /// <returns>The test</returns>
    public static TTest AddSources<TTest>(this TTest test, params string[] sources)
        where TTest : AnalyzerTest<Verifier>
    {
        foreach (var source in sources)
        {
            test.TestState.Sources.Add(source);
        }

        return test;
    }

    /// <summary>
    /// Adds assembly references to the test compilation
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="localReferences">The assemblies to reference</param>
    /// <returns>The test</returns>
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

    /// <summary>
    /// Sets the reference assemblies for the test compilation.
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="referenceAssemblies">The reference assemblies to use</param>
    /// <returns>The test</returns>
    public static TTest WithReferenceAssemblies<TTest>(this TTest test, ReferenceAssemblies referenceAssemblies)
        where TTest : AnalyzerTest<Verifier>
    {
        test.ReferenceAssemblies = referenceAssemblies.AddPackages(test.ReferenceAssemblies.Packages);
        return test;
    }

    /// <summary>
    /// Sets the language version used by the test compilation.
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="languageVersion">The language version to use</param>
    /// <returns>The test</returns>
    public static TTest WithLanguageVersion<TTest>(this TTest test, LanguageVersion languageVersion)
        where TTest : AnalyzerTest<Verifier>
    {
        test.SolutionTransforms.Add((solution, projectId) => solution.WithProjectParseOptions(projectId, new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose)));
        return test;
    }

    /// <summary>
    /// Updates the compilation options of the test compilation.
    /// </summary>
    /// <typeparam name="TTest">The test type</typeparam>
    /// <param name="test">The test</param>
    /// <param name="callback">The callback to update the compilation options</param>
    /// <returns>The test</returns>
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