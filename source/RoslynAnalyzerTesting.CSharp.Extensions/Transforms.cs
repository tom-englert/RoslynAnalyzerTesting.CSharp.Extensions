using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynAnalyzerTesting.CSharp.Extensions;

public static class Transforms
{
    public static Func<Solution, ProjectId, Solution> CreateSolutionTransform(Func<Solution, Project, Solution> transform)
    {
        return (solution, projectId) =>
        {
            var project = solution.GetProject(projectId) ?? throw new InvalidOperationException("Project is not part of the solution");
            return transform(solution, project);
        };
    }

    public static Func<Solution, ProjectId, Solution> AddAssemblyReferences(params Assembly[] assemblies) => (solution, projectId) =>
    {
        var metadataReferences = assemblies
            .Distinct()
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

        return solution.AddMetadataReferences(projectId, metadataReferences);
    };

    public static Func<Solution, ProjectId, Solution> UseLanguageVersion(LanguageVersion languageVersion) => CreateSolutionTransform((solution, project) =>
    {
        var parseOptions = project.ParseOptions as CSharpParseOptions ?? throw new InvalidOperationException("Project does not have CSharpParseOptions");

        return solution.WithProjectParseOptions(project.Id, parseOptions.WithLanguageVersion(languageVersion));
    });

    public static Func<Solution, ProjectId, Solution> WithProjectCompilationOptions(Func<CSharpCompilationOptions, CSharpCompilationOptions> callback) => CreateSolutionTransform((solution, project) =>
    {
        var compilationOptions = project.CompilationOptions as CSharpCompilationOptions ?? throw new InvalidOperationException("Project does not have CSharpCompilationOptions");

        return solution.WithProjectCompilationOptions(project.Id, callback(compilationOptions));
    });
}