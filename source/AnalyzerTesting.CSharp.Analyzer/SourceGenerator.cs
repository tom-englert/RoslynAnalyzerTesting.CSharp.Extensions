using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnalyzerTesting.CSharp.Analyzer;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var verifierTypeProvider = context.CompilationProvider.Select(FindVerifierTypes);

        context.RegisterSourceOutput(verifierTypeProvider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, string verifierTypes)
    {
        var template = GetTemplate(verifierTypes);

        context.AddSource("AnalyzerTesting.CSharp.FluentExtensions.g.cs", template.Replace("%VERIFIER%", verifierTypes));
    }

    private static string GetTemplate(string verifierTypes)
    {
        if (string.IsNullOrWhiteSpace(verifierTypes))
            return GeneratedSourceTemplate.NoVerifierFound;

        if (verifierTypes.Contains(';'))
            return GeneratedSourceTemplate.MultipleVerifiersFound;

        return GeneratedSourceTemplate.Text;
    }

    private static string FindVerifierTypes(Compilation compilation, CancellationToken cancellationToken)
    {
        HashSet<string> verifierTypeCandidates = new(StringComparer.Ordinal);

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var walker = new SyntaxWalker(compilation, syntaxTree, verifierTypeCandidates);

            walker.GetVerifierTypeCandidates();
        }

        return string.Join(";", verifierTypeCandidates.OrderBy(item => item));
    }

    private class SyntaxWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly SyntaxTree _syntaxTree;
        private readonly HashSet<string> _verifierTypeCandidates;

        public SyntaxWalker(Compilation compilation, SyntaxTree syntaxTree, HashSet<string> verifierTypeCandidates)
        {
            _syntaxTree = syntaxTree;
            _verifierTypeCandidates = verifierTypeCandidates;
            _semanticModel = compilation.GetSemanticModel(syntaxTree);
        }

        public void GetVerifierTypeCandidates()
        {
            Visit(_syntaxTree.GetRoot());
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            GetVerifierTypeName(_semanticModel.GetSymbolInfo(node.Type).Symbol as INamedTypeSymbol);

            base.VisitObjectCreationExpression(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            GetVerifierTypeName(_semanticModel.GetDeclaredSymbol(node));

            base.VisitClassDeclaration(node);
        }

        private void GetVerifierTypeName(INamedTypeSymbol? typeSymbol)
        {
            if (typeSymbol == null)
                return;

            var rootType = typeSymbol.GetTypeHierarchy().Last();

            if (rootType is not {Name: "AnalyzerTest", IsGenericType: true, TypeArguments.Length: 1}) 
                return;

            if (rootType.GetNamespace() != "Microsoft.Microsoft.CodeAnalysis.Microsoft.CodeAnalysis.Testing") 
                return;


            var verifierType = rootType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            _verifierTypeCandidates.Add(verifierType);
        }
    }
}