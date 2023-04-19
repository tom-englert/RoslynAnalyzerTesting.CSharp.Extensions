using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AnalyzerTesting.CSharp.Extensions;

/// <summary>
/// Commonly used solution transforms
/// </summary>
public static class Transforms
{
    /// <summary>
    /// Create a solution transform with the project instead of just the project id
    /// </summary>
    /// <param name="transform">The transform to apply</param>
    /// <returns>The transform</returns>
    public static Func<Solution, ProjectId, Solution> CreateSolutionTransform(Func<Solution, Project, Solution> transform)
    {
        return (solution, projectId) =>
        {
            var project = solution.GetProject(projectId) ?? throw new InvalidOperationException("Project is not part of the solution");
            return transform(solution, project);
        };
    }

    /// <summary>
    /// A solution transform that adds assembly references to the test project.
    /// </summary>
    /// <param name="assemblies">The assemblies to reference</param>
    /// <returns>The transform</returns>
    public static Func<Solution, ProjectId, Solution> AddAssemblyReferences(params Assembly[] assemblies) => (solution, projectId) =>
    {
        var metadataReferences = assemblies
            .Distinct()
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        return solution.AddMetadataReferences(projectId, metadataReferences);
    };

    /// <summary>
    /// A solution transform that sets the language version used for the test compilation.
    /// </summary>
    /// <param name="languageVersion"></param>
    /// <returns>The transform</returns>
    public static Func<Solution, ProjectId, Solution> UseLanguageVersion(LanguageVersion languageVersion) => CreateSolutionTransform((solution, project) =>
    {
        var parseOptions = project.ParseOptions as CSharpParseOptions ?? throw new InvalidOperationException("Project does not have CSharpParseOptions");

        return solution.WithProjectParseOptions(project.Id, parseOptions.WithLanguageVersion(languageVersion));
    });

    /// <summary>
    /// A solution transform that updates the project compilation options.
    /// </summary>
    /// <param name="callback">The callback to update the compilation options</param>
    /// <returns>The transform</returns>
    public static Func<Solution, ProjectId, Solution> WithProjectCompilationOptions(Func<CSharpCompilationOptions, CSharpCompilationOptions> callback) => CreateSolutionTransform((solution, project) =>
    {
        var compilationOptions = project.CompilationOptions as CSharpCompilationOptions ?? throw new InvalidOperationException("Project does not have CSharpCompilationOptions");

        return solution.WithProjectCompilationOptions(project.Id, callback(compilationOptions));
    });
}