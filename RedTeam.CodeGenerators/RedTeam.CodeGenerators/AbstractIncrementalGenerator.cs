using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RedTeam.CodeGenerators.Models;
using RedTeam.Extensions.CodeGenerators.Models;
using System.Collections.Immutable;

namespace RedTeam.CodeGenerators;

public abstract class AbstractIncrementalGenerator<TContext> : IIncrementalGenerator where TContext : GenerationContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
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


        //var methodName = context.AnalyzerConfigOptionsProvider
        //.Select(static (c, _) =>
        //{

        //    c.GlobalOptions.TryGetValue("build_property.AddCommandHandler", out var methodName);
        //    c.GlobalOptions.TryGetValue("build_property.AddCommandHandlerInternal", out var methodInternal);
        //    return new MethodOptions(methodName, methodInternal);
        //});

        var methodName = GetMethodOptionsFromBuildProperties(context.AnalyzerConfigOptionsProvider);

        var options = assemblyName.Combine(methodName);

        var generation = registrations.Combine(options);

        context.RegisterSourceOutput(generation, ExecuteGeneration);

    }

    protected abstract IncrementalValueProvider<MethodOptions> GetMethodOptionsFromBuildProperties(IncrementalValueProvider<AnalyzerConfigOptionsProvider> provider);

    protected abstract void ExecuteGeneration(
  SourceProductionContext sourceContext,
    (ImmutableArray<TContext?> Registrations, (string? AssemblyName, MethodOptions? MethodOptions) Options) source);

    protected abstract TContext? SemanticTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken);

    protected abstract bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken);

    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic>? diagnostics)
    {
        if (diagnostics == null)
            return;

        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }
}