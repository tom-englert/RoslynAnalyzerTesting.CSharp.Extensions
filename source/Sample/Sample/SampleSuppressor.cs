using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sample
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SampleSuppressor : DiagnosticSuppressor
    {
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                var id = diagnostic.Id;
                var descriptor = SupportedSuppressions.Single(s => s.SuppressedDiagnosticId == id);
                context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(
            new SuppressionDescriptor("SMPL002", SampleAnalyzer.SampleDiagnostic.Id, "Just suppress everything for demo"),
            new SuppressionDescriptor("SMPL003", "CS8618", "Just suppress everything for demo")
            );
    }
}
