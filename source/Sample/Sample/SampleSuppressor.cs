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
                context.ReportSuppression(Suppression.Create(SupportedSuppressions[0], diagnostic));
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(new SuppressionDescriptor("SMPL002", SampleAnalyzer.SampleDiagnostic.Id, "Just suppress everything for demo"));
    }
}
