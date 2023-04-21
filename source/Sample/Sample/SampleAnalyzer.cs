using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sample;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SampleAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor SampleDiagnostic = new("SMPL01", "Sample", "Message {0}", "Sample", DiagnosticSeverity.Warning, true);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private void Analyze(SymbolAnalysisContext context)
    {
        var symbol = (INamedTypeSymbol)context.Symbol;

        if (!symbol.Name.ToCharArray().Any(char.IsLower))
            return;

        context.ReportDiagnostic(Diagnostic.Create(SampleDiagnostic, symbol.Locations[0], symbol.Name));
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(SampleDiagnostic);
}
