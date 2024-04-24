using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace TotkRegistryToolkit.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class FeatureGenerator : IIncrementalGenerator
{
    private const string FEATURE_INTERFACE_TYPE_NAME = "TotkRegistryToolkit.Services.IFeature";
    private const string FEATURE_ATTRIBUTE_TYPE_NAME = "TotkRegistryToolkit.Attributes.FeatureAttribute";

    private static readonly Diagnostic _mergerInterfaceNotFoundError = Diagnostic.Create(new(
        "TRTK0001",
        "Feature Interface Not Found",
        "The IFeature interface could not be found in the target assembly",
        "Problem",
        DiagnosticSeverity.Error,
        true
    ), Location.None, DiagnosticSeverity.Error);

    private static readonly DiagnosticDescriptor _mergerInterfaceNotImplementedError = new(
        "TRTKC0002",
        "Feature Interface Not Implemented",
        "Classes using the FeatureAttribute must implement IFeature",
        "Problem",
        DiagnosticSeverity.Error,
        true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> provider = context.SyntaxProvider.ForAttributeWithMetadataName(FEATURE_ATTRIBUTE_TYPE_NAME,
            predicate: (n, _) => n is ClassDeclarationSyntax,
            transform: (n, _) => (ClassDeclarationSyntax)n.TargetNode
        );

        IncrementalValueProvider<(Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right)> compilation
            = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation,
            (spc, source) => Execute(spc, source.Left, source.Right)
        );
    }

    private void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> typeList)
    {
#if DEBUG
        // if (!System.Diagnostics.Debugger.IsAttached) {
        //     System.Diagnostics.Debugger.Launch();
        // }
#endif

        if (compilation.GetTypeByMetadataName(FEATURE_INTERFACE_TYPE_NAME) is not INamedTypeSymbol mergerInterfaceSymbol) {
            context.ReportDiagnostic(_mergerInterfaceNotFoundError);
            return;
        }

        INamedTypeSymbol mergerAttributeSymbol = compilation.GetTypeByMetadataName(FEATURE_ATTRIBUTE_TYPE_NAME)!;

        StringBuilder switchExpressionBuilder = new();
        StringBuilder featureListBuilder = new();

        foreach (ClassDeclarationSyntax syntax in typeList) {
            if (compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax) is not INamedTypeSymbol symbol) {
                continue;
            }

            if (!symbol.Interfaces.Contains(mergerInterfaceSymbol)) {
                context.ReportDiagnostic(
                    Diagnostic.Create(_mergerInterfaceNotImplementedError, syntax.GetLocation(), DiagnosticSeverity.Error)
                );
                continue;
            }

            AttributeData attribute = symbol.GetAttributes()
                .First(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, mergerAttributeSymbol));

            object? name = attribute.ConstructorArguments[0].Value;
            switchExpressionBuilder.AppendLine($"\"{name}\" => global::{symbol.ContainingNamespace}.{symbol.Name}.Execute(args),");
            switchExpressionBuilder.Append("                ");

            object? description = attribute.ConstructorArguments[1].Value;
            featureListBuilder.AppendLine($"(\"{name}\", \"{description}\")");
            switchExpressionBuilder.Append("            ");
        }

        string code = $$"""
            #nullable enable

            namespace TotkRegistryToolkit.Services
            {
                public static partial class FeatureService
                {
                    public static readonly (string, string)[] Features = [
                        {{featureListBuilder}}
                    ];

                    public static partial async ValueTask Execute(global::System.String name, global::System.String[] args)
                    {
                        ValueTask task = name switch {
                            {{switchExpressionBuilder}}_ => throw new global::System.InvalidOperationException($"Unexpected feature name: '{name}'")
                        };

                        await task;
                    }
                }
            }
            """;

        context.AddSource("TotkRegistryToolkit.Services.FeatureService.g.cs", code);
    }
}
