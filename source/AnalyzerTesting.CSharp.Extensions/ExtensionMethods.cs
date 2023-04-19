using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerTesting.CSharp.Extensions;

public static class ExtensionMethods
{
    public static DiagnosticResult AsResult(this DiagnosticDescriptor descriptor) => new(descriptor);

    public static CSharpCompilationOptions AsCSharpCompilationOptions(this CompilationOptions options) => (CSharpCompilationOptions)options;

    public static CSharpCompilationOptions WithCSharpDefaults(this CompilationOptions options) => options
        .AsCSharpCompilationOptions()
        .WithNullableWarningsAsErrors()
        .WithNullableContextOptions(NullableContextOptions.Enable);

    public static ReferenceAssemblies AddPackages(this ReferenceAssemblies referenceAssemblies, params PackageIdentity[] packages)
    {
        return referenceAssemblies.AddPackages(packages.ToImmutableArray());
    }

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