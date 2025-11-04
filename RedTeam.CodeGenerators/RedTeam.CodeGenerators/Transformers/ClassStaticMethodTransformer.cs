using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RedTeam.Extensions.CodeGenerators.Helpers;
using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.Extensions.CodeGenerators.Transformers;
internal static class ClassStaticMethodTransformer
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <returns>AddKafkaFlowContext or null</returns>
    public static AddCommandHandlerContext? ExtractCommandHandlerRegistrationAttribute(GeneratorSyntaxContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
            return null;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
        if (methodSymbol is null)
            return null;

        // make sure attribute is for registration
        var attributes = methodSymbol.GetAttributes();
        var isKnown = attributes.Any(AttributeHelper.IsMethodRegistrationAttribute);
        if (!isKnown)
            return null;


        //get the class
        SyntaxNode? syntaxNode = methodDeclaration.Parent;
        INamedTypeSymbol? classSymbol = null;


        while (syntaxNode != null)
        {
            //what type of node is this?
            if (syntaxNode.IsKind(SyntaxKind.ClassDeclaration))
            {
                var classDeclartion = syntaxNode as ClassDeclarationSyntax;
                if (classDeclartion != null)
                {
                    classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclartion);
                    break;
                }
            }
            else
            {
                syntaxNode = syntaxNode.Parent;
            }
        }

        var (diagnostics, hasCoconaApp) = ValidateMethod(methodDeclaration, methodSymbol, classSymbol);
        if (diagnostics.Any())
            return new AddCommandHandlerContext(diagnostics);

        var registration = new MethodRegistration
        (
            ClassName: methodSymbol.ContainingType.ToDisplayString(Constants.FullyQualifiedNullableFormat),
            MethodName: methodSymbol.Name
        );

        return new AddCommandHandlerContext(MethodRegistrations: new[] { registration });
    }

    private static (EquatableArray<Diagnostic> diagnostics, bool hasCoconaApp)
        ValidateMethod(MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol, INamedTypeSymbol? namedTypeSymbol)
    {
        var diagnostics = new List<Diagnostic>();
        var hasCoconaApp = false;

        var methodName = methodSymbol.Name;

        if (!methodSymbol.IsStatic)
        {
            var diagnostic = Diagnostic.Create(
                                AddCommandHandlerDiagnosticDescriptors.StaticMethod,
                                methodDeclaration.GetLocation(),
                                methodName
                            );
            diagnostics.Add(diagnostic);
        }


        var isCommandHandler = namedTypeSymbol?.IsCommandHandler() ?? false;



        if (isCommandHandler)
        {

            // validate first parameter should be KafkaClientBuilder
            if (methodSymbol.Parameters.Length is 1)
            {
                var parameterSymbol = methodSymbol.Parameters[0];
                hasCoconaApp = parameterSymbol.Type.IsCoconaAppType();
                if (!hasCoconaApp)
                {
                    var diagnostic = Diagnostic.Create(
                        AddCommandHandlerDiagnosticDescriptors.InvalidCoconaAppParameter,
                        methodDeclaration.GetLocation(),
                        parameterSymbol.Name,
                        methodName
                    );
                    diagnostics.Add(diagnostic);
                }
            }



            if (methodSymbol.Parameters.Length is 1)
                return (diagnostics.ToArray(), hasCoconaApp);

            // invalid parameter count
            var parameterDiagnostic = Diagnostic.Create(
                AddCommandHandlerDiagnosticDescriptors.TooManyParameter,
                methodDeclaration.GetLocation(),
                methodName
            );
            diagnostics.Add(parameterDiagnostic);

            return (diagnostics.ToArray(), hasCoconaApp);
        }

        //attribute applied to an invalid class
        var invalidClassDiagnostic = Diagnostic.Create(
                               AddCommandHandlerDiagnosticDescriptors.InvalidClass,
                               methodDeclaration.GetLocation(),
                               methodName
                           );
        diagnostics.Add(invalidClassDiagnostic);

        return (diagnostics.ToArray(), hasCoconaApp);
    }
}



