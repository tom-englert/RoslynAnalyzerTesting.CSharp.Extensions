using Microsoft.CodeAnalysis;

namespace AnalyzerTesting.CSharp.Analyzer;

internal static class ExtensionMethods
{
    public static IEnumerable<INamedTypeSymbol> GetTypeHierarchy(this INamedTypeSymbol symbol)
    {
        while (symbol.BaseType != null)
        {
            yield return symbol;
            symbol = symbol.BaseType;
        }
    }

    public static IEnumerable<INamespaceSymbol> GetNamespaceHierarchy(this INamedTypeSymbol symbol)
    {
        var namespaceSymbol = symbol.ContainingNamespace;
        while (namespaceSymbol is { IsGlobalNamespace: false})
        {
            yield return namespaceSymbol;
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }
    }

    public static string GetNamespace(this INamedTypeSymbol symbol)
    {
        return string.Join(".", symbol.GetNamespaceHierarchy().Reverse());
    }
}