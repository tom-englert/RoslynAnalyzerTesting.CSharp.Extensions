# Roslyn analyzer testing extensions

## Instantly start coding roslyn analyzer tests, without having to prepare a new derived class for every test flavor.

This project provides solution transforms and extension methods to simpify testing of roslyn analyzers 
and to ease configuration of the compilation like adding references or changing language version.

It also provides base classes to test incremental generators and diagnostic suppressors with `"Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` version `1.1.1` and `Microsoft.CodeAnalysis.CSharp.Workspaces` >= `4.3.0`


- Predefined solution transforms to fine tune the test when using object initializer notation
- Extension methods to configure the test using fluent notation
- An `IncrementalSourceGenerator` test to test incremental source generators with `"Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` version `1.1.1`
- An `IncrementalSourceGenerator` test that supports snapshot testing
- A `DiagnosticSuppressor` test to test diagnostic suppressors with `Microsoft.CodeAnalysis.CSharp.Workspaces` >= `4.3.0`

