using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Extensions;

/// <summary>
/// Extension methods to simplify writing of analyzer tests.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Converts a <see cref="DiagnosticDescriptor"/> into a <see cref="DiagnosticResult"/>
    /// </summary>
    /// <param name="descriptor">The <see cref="DiagnosticDescriptor"/></param>
    /// <returns>The new <see cref="DiagnosticResult"/></returns>
    public static DiagnosticResult AsResult(this DiagnosticDescriptor descriptor) => new(descriptor);

    /// <summary>
    /// Up-casts <see cref="CompilationOptions"/> to <see cref="CSharpCompilationOptions"/>
    /// </summary>
    /// <param name="options">The <see cref="CompilationOptions"/></param>
    /// <returns>The <see cref="CSharpCompilationOptions"/></returns>
    public static CSharpCompilationOptions AsCSharpCompilationOptions(this CompilationOptions options) => (CSharpCompilationOptions)options;

    /// <summary>
    /// Adds C# defaults to the compilation options: Enable NRTs and set NullableWarningsAsErrors.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static CSharpCompilationOptions WithCSharpDefaults(this CompilationOptions options) => options
        .AsCSharpCompilationOptions()
        .WithNullableWarningsAsErrors()
        .WithNullableContextOptions(NullableContextOptions.Enable);

    /// <summary>
    /// Adds nuget packages to the <see cref="ReferenceAssemblies"/>
    /// </summary>
    /// <param name="referenceAssemblies">The <see cref="ReferenceAssemblies"/></param>
    /// <param name="packages">The packages to add</param>
    /// <returns>The updated <see cref="ReferenceAssemblies"/></returns>
    public static ReferenceAssemblies AddPackages(this ReferenceAssemblies referenceAssemblies, params PackageIdentity[] packages)
    {
        return referenceAssemblies.AddPackages(packages.ToImmutableArray());
    }

    /// <summary>
    /// Sets the severity of NRT warnings to error.
    /// </summary>
    /// <param name="options">The <see cref="CSharpCompilationOptions"/></param>
    /// <returns>The updated <see cref="CSharpCompilationOptions"/></returns>
    public static CSharpCompilationOptions WithNullableWarningsAsErrors(this CSharpCompilationOptions options)
    {
        return options.WithSpecificDiagnosticOptions(options.SpecificDiagnosticOptions.SetItems(NullableWarningsAsErrors));
    }

    /// <summary>
    /// By default, the compiler reports diagnostics for nullable reference types at
    /// <see cref="DiagnosticSeverity.Warning"/>, and the analyzer test framework defaults to only validating
    /// diagnostics at <see cref="DiagnosticSeverity.Error"/>. This map contains all compiler diagnostic IDs
    /// related to nullability mapped to <see cref="ReportDiagnostic.Error"/>, which is then used to enable all
    /// of these warnings for default validation during analyzer and code fix tests.
    /// </summary>
    public static ImmutableDictionary<string, ReportDiagnostic> NullableWarningsAsErrors { get; } = GetNullableWarningsFromCompiler();

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = { "/warnaserror:nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
        return nullableWarnings;
    }
}