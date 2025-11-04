using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RedTeam.Extensions.CodeGenerators.Models;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace RedTeam.CodeGenerators.Generators.Enumerations;

[Generator]
public class EnumGenerator : AbstractIncrementalGenerator<EnumGeneratorContext>
{
    protected override void ExecuteGeneration(SourceProductionContext sourceContext, (ImmutableArray<EnumGeneratorContext?> Registrations, (string? AssemblyName, MethodOptions? MethodOptions) Options) source)
    {

        var enumRegistrations = source.Registrations
          .SelectMany(m => m?.EnumRegistrations ?? Array.Empty<EnumRegistration>())
          .Where(m => m is not null)
          .ToArray();

        // compute extension method name
        var methodName = source.Options.MethodOptions?.Name;
        if (string.IsNullOrWhiteSpace(methodName))
            methodName = $"{Regex.Replace(source.Options.AssemblyName, "\\W", "")}Enums";

        var methodInternal = source.Options.MethodOptions?.Internal;

        var result = EnumWriter.GenerateExtensionClass(enumRegistrations, source.Options.AssemblyName!, methodName!, methodInternal!);

        sourceContext.AddSource("RedTeam.Enums.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    protected override IncrementalValueProvider<MethodOptions> GetMethodOptionsFromBuildProperties(IncrementalValueProvider<AnalyzerConfigOptionsProvider> provider)
    {
        return provider
     .Select(static (c, _) =>
     {
         c.GlobalOptions.TryGetValue("build_property.EnumGenerator", out var methodName);
         c.GlobalOptions.TryGetValue("build_property.EnumGeneratorInternal", out var methodInternal);
         return new MethodOptions(methodName, methodInternal);
     });

    }

    protected override EnumGeneratorContext? SemanticTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        return context.Node switch
        {
            // We can only apply attributes to enum declarations
            EnumDeclarationSyntax enumDeclarationSyntax => EnumTransformer.Create(context, enumDeclarationSyntax, cancellationToken),
            _ => null
        };
    }

    protected override bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        /// We are only interested in enum declarations with attributes
        return syntaxNode is EnumDeclarationSyntax { AttributeLists.Count: > 0 };
    }
}
