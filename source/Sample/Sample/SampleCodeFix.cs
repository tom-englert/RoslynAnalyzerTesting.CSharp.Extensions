using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Composition;

namespace Sample;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SampleCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SampleAnalyzer.SampleDiagnostic.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
        if (declaration == null)
            return;

        context.RegisterCodeFix(CodeAction.Create("SampleFix", c => MakeUppercaseAsync(context.Document, declaration, c), nameof(SampleCodeFixProvider)), diagnostic);
    }

    private static async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
    {
        var identifierToken = typeDecl.Identifier;
        var newName = identifierToken.Text.ToUpperInvariant();

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var typeSymbol = semanticModel?.GetDeclaredSymbol(typeDecl, cancellationToken);
        var originalSolution = document.Project.Solution;

        if (typeSymbol == null)
            return originalSolution;

        var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, new SymbolRenameOptions(), newName, cancellationToken).ConfigureAwait(false);

        return newSolution;
    }
}

