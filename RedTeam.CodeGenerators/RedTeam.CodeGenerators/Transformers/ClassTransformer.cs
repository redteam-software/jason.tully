using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RedTeam.Extensions.CodeGenerators.Helpers;
using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.Extensions.CodeGenerators.Transformers;
internal static class ClassTransformer
{
    /// <summary>
    /// From the given syntax context, attempts to extract a registration attributes for handlers.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static AddCommandHandlerContext? ExtractAddCommandRegistrationsFromClassAttributes(GeneratorSyntaxContext context)



    {
        var diagnostics = new List<Diagnostic>();

        bool isClass = context.Node is ClassDeclarationSyntax classDeclaration;

        if (!isClass)
        {
            return null;
        }


        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;

        if (symbol is null)
            return null;



        var attributes = symbol.GetAttributes();

        var commandHandlerRegistrations = new List<CommandHandlerRegistration>();
        var subCommandHandlerRegistrations = new List<SubCommandHandlerRegistration>();
        var subCommandRegistrations = new List<SubCommandRegistration>();




        foreach (var attribute in attributes)
        {
            var registration = CreateCommandHandlerRegistrationFromAttribute(symbol, attribute);

            if (registration is not null)
            {
                commandHandlerRegistrations.Add(registration);
            }


            var subCommand = CreateSubCommandHandlerRegistrationFromAttribute(symbol, attribute);

            if (subCommand is not null)
            {
                subCommandHandlerRegistrations.Add(subCommand);
            }


            var sub = CreateSubCommandFromAttribute(symbol, attribute);
            if (sub != null)
            {
                subCommandRegistrations.Add(sub);
            }

        }


        return new AddCommandHandlerContext(
            CommandHandlerRegistrations: commandHandlerRegistrations.ToArray(),
            SubCommandHandlerRegistrations: subCommandHandlerRegistrations.ToArray(),
            SubCommandRegistrations: subCommandRegistrations.ToArray());
    }
    private static SubCommandRegistration? CreateSubCommandFromAttribute(INamedTypeSymbol? classSymbol, AttributeData attribute)
    {
        if (!AttributeHelper.IsSubCommandAttribute(attribute))
        {
            return null;
        }
        var className = classSymbol!.ToDisplayString(Constants.FullyQualifiedNullableFormat);

        string? commandName = null;
        string? commandDescription = null;
        string? parentCommand = null;


        if (attribute.ConstructorArguments.Length is 2 or 3)
        {

            commandName = attribute.ConstructorArguments[0].Value!.ToString();
            commandDescription = attribute.ConstructorArguments[1].Value!.ToString();

            if (attribute.ConstructorArguments.Length == 3)
            {
                parentCommand = attribute.ConstructorArguments[2].Value?.ToString();
            }


            return new SubCommandRegistration(
            className!,
            commandName!,
            commandDescription!,
            parentCommand);
        }

        return null;


    }
    private static SubCommandHandlerRegistration? CreateSubCommandHandlerRegistrationFromAttribute(INamedTypeSymbol? classSymbol, AttributeData attribute)
    {
        if (!AttributeHelper.IsSubCommandHandlerRegistrationAttribute(attribute))
        {
            return null;
        }

        var registration = GetSharedRegistration(classSymbol, attribute);

        if (registration is null)
        {
            return default;
        }

        string? parentCommandName = null;
        string? commandName = null;
        string? commandDescription = null;

        if (attribute.ConstructorArguments.Length == 3)
        {
            parentCommandName = attribute.ConstructorArguments[0].Value!.ToString();

            commandName = attribute.ConstructorArguments[1].Value!.ToString();
            commandDescription = attribute.ConstructorArguments[2].Value!.ToString();


            return new SubCommandHandlerRegistration(
            parentCommandName,
            registration.CommandHandlerClassName,
            registration.CommandParametersClassName,
            commandName!,
            commandDescription!,
            registration.RegistrationMethodName);
        }

        return null;


    }
    private static CommandHandlerRegistration? CreateCommandHandlerRegistrationFromAttribute(INamedTypeSymbol? classSymbol, AttributeData attribute)
    {
        if (!AttributeHelper.IsCommandHandlerRegistrationAttribute(attribute))
        {
            return null;
        }

        var registration = GetSharedRegistration(classSymbol, attribute);

        if (registration is null)
        {
            return default;
        }


        string? commandName = null;
        string? commandDescription = null;

        if (attribute.ConstructorArguments.Length == 2)
        {
            commandName = attribute.ConstructorArguments[0].Value!.ToString();
            commandDescription = attribute.ConstructorArguments[1].Value!.ToString();





            return new CommandHandlerRegistration(
                registration.CommandHandlerClassName,
                registration.CommandParametersClassName,
                commandName!,
                commandDescription!,
                registration.RegistrationMethodName);
        }

        return null;


    }

    private static Registration? GetSharedRegistration(INamedTypeSymbol? classSymbol, AttributeData attribute)
    {
        if (classSymbol is null)
        {
            return null;
        }

        var commandParameterType = ExtractMessageTypeFromBaseClass(classSymbol);

        string? configurationType = null;

        var attributeClass = attribute.AttributeClass;


        if (attributeClass is { IsGenericType: true } && attributeClass.TypeArguments.Length == attributeClass.TypeParameters.Length)
        {
            // if generic attribute, get the message type (json or protobuf)
            for (var index = 0; index < attributeClass.TypeParameters.Length; index++)
            {
                var typeParameter = attributeClass.TypeParameters[index];
                var typeArgument = attributeClass.TypeArguments[index];



                if (typeParameter.Name == "ICommand" || index == 1)
                {
                    configurationType = typeArgument.ToDisplayString(Constants.FullyQualifiedNullableFormat);

                }
            }
        }



        var registrationMethodName = attribute!.AttributeClass!.Name.Replace("Attribute", string.Empty);
        return new Registration(classSymbol!.ToDisplayString(Constants.FullyQualifiedNullableFormat),
            commandParameterType!, registrationMethodName);
    }


    private static string? ExtractMessageTypeFromBaseClass(INamedTypeSymbol classSymbol)
    {
        var i = classSymbol.Interfaces.FirstOrDefault();
        INamedTypeSymbol? baseType = null;
        if (i != null)
        {
            baseType = i;
        }
        else
        {
            baseType = classSymbol.BaseType;
        }

        var name = baseType!.Name;

        if (baseType is { IsGenericType: true } && baseType.TypeParameters.Length == 1)
        {

            var typeParameter = baseType.TypeParameters[0];
            var typeArgument = baseType.TypeArguments[0];

            return typeArgument.ToDisplayString(Constants.FullyQualifiedNullableFormat);


        }

        return null;
    }

}
