using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RedTeam.Extensions.CodeGenerators.Models;
using RedTeam.Extensions.CodeGenerators.Transformers;

namespace RedTeam.Extensions.CodeGenerators;

[Generator]
public class AddCommandHandlerGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        var pipeline = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: SyntacticPredicate,
                transform: SemanticTransform
            )
            .Where(static context => context is not null);

        // Emit the diagnostics, if needed
        var diagnostics = pipeline
            .Select(static (item, _) => item!.Diagnostics)
            .Where(static item => item?.Count > 0);

        context.RegisterSourceOutput(diagnostics, ReportDiagnostic);


        var registrations = pipeline
         .Where(static context => context?.Valid == true)
         .Collect();

        // include config options
        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName);

        var methodName = context.AnalyzerConfigOptionsProvider
        .Select(static (c, _) =>
        {
            c.GlobalOptions.TryGetValue("build_property.AddCommandHandler", out var methodName);
            c.GlobalOptions.TryGetValue("build_property.AddCommandHandlerInternal", out var methodInternal);
            return new MethodOptions(methodName, methodInternal);
        });

        var options = assemblyName.Combine(methodName);

        var generation = registrations.Combine(options);

        context.RegisterSourceOutput(generation, ExecuteGeneration);
    }
    private static AddCommandHandlerContext? SemanticTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {

        return context.Node switch
        {
            ClassDeclarationSyntax => ClassTransformer.ExtractAddCommandRegistrationsFromClassAttributes(context),
            MethodDeclarationSyntax => ClassStaticMethodTransformer.ExtractCommandHandlerRegistrationAttribute(context),
            _ => null
        };
    }

    private void ExecuteGeneration(
  SourceProductionContext sourceContext,
    (ImmutableArray<AddCommandHandlerContext?> Registrations, (string? AssemblyName, MethodOptions? MethodOptions) Options) source)
    {

        var handlerRegistrations = source.Registrations
          .SelectMany(m => m?.CommandHandlerRegistrations ?? Array.Empty<CommandHandlerRegistration>())
          .Where(m => m is not null)
          .ToArray();

        var subCommandHandlerRegistrations = source.Registrations
        .SelectMany(m => m?.SubCommandHandlerRegistrations ?? Array.Empty<SubCommandHandlerRegistration>())
        .Where(m => m is not null)
        .ToArray();

        var subCommandRegistrations = source.Registrations
      .SelectMany(m => m?.SubCommandRegistrations ?? Array.Empty<SubCommandRegistration>())
      .Where(m => m is not null)
      .ToArray();

        var methodRegistrations = source.Registrations
            .SelectMany(m => m?.MethodRegistrations ?? Array.Empty<MethodRegistration>())
            .Where(m => m is not null)
            .ToArray();


        // compute extension method name
        var methodName = source.Options.MethodOptions?.Name;
        if (string.IsNullOrWhiteSpace(methodName))
            methodName = $"{Regex.Replace(source.Options.AssemblyName, "\\W", "")}CommandHandlers";

        var methodInternal = source.Options.MethodOptions?.Internal;


        // generate registration method
        var result = AddCommandHandlerWriter.GenerateExtensionClass(handlerRegistrations,
            subCommandHandlerRegistrations,
            subCommandRegistrations,
            methodRegistrations,
            source.Options.AssemblyName!, methodName!, methodInternal!);

        // add source file

        sourceContext.AddSource("RedTeam.Extensions.Console.g.cs", SourceText.From(result, Encoding.UTF8));

    }




    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclaration
                   && !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
                   && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                   || (syntaxNode is MemberDeclarationSyntax { AttributeLists.Count: > 0 } memberDeclaration
                   && !memberDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword));
    }
    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic>? diagnostics)
    {
        if (diagnostics == null)
            return;

        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }



}
