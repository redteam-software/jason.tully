using Microsoft.CodeAnalysis;

namespace RedTeam.Extensions.CodeGenerators.Helpers;
static class TypeHelper
{

    /// <summary>
    /// Determines if a method parameter is a command handler
    /// </summary>
    /// <param name="parameterSymbol"></param>
    /// <returns></returns>
    public static bool IsCommandHandler(this IParameterSymbol parameterSymbol) => IsCommandHandler(parameterSymbol.Type);

    /// <summary>
    /// Determines if a class is a command handler
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public static bool IsCommandHandler(this ITypeSymbol? typeSymbol)

    {
        if (typeSymbol is null)
        {
            return false;
        }

        var interfaceDefinition = typeSymbol.Interfaces.Any(x => x is
        {
            Name: "ICommand",
            ContainingNamespace:
            {
                Name: "Commands",
                ContainingNamespace:
                {
                    Name: "Console",
                    ContainingNamespace:
                    {
                        Name: "Extensions",
                        ContainingNamespace.Name: "RedTeam"
                    }
                }
            }

        });

        if (interfaceDefinition)
        {
            return interfaceDefinition;
        }


        return typeSymbol is
        {
            Name: "ICommand",
            ContainingNamespace:
            {
                Name: "Commands",
                ContainingNamespace:
                {
                    Name: "Console",
                    ContainingNamespace:
                    {
                        Name: "Extensions",
                        ContainingNamespace.Name: "RedTeam"
                    }
                }
            }

        };


    }

    public static bool IsCoconaAppType(this ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        return typeSymbol is
        {
            Name: "CoconaApp"

        };
    }
    /// <summary>
    /// Determines if a method paramater is an IKafaConfig or descendant.
    /// </summary>
    /// <param name="parameterSymbol"></param>
    /// <returns></returns>
    public static bool IsKafkaEventConfiguration(this IParameterSymbol parameterSymbol)
    {
        var type = parameterSymbol.Type;
        var implementsInterface = type.AllInterfaces.Any(x => x is
        {
            Name: "IKafkaConfig",
            ContainingNamespace:
            {
                Name: "Configuration",
                ContainingNamespace:
                {
                    Name: "KafkaFlow",
                    ContainingNamespace.Name: "RedTeam"
                }
            }
        });

        var extendsBaseClass = type.BaseType is
        {
            Name: "KafkaConfig",
            ContainingNamespace:
            {
                Name: "Configuration",
                ContainingNamespace:
                {
                    Name: "KafkaFlow",
                    ContainingNamespace.Name: "RedTeam"
                }
            }
        };

        return implementsInterface || extendsBaseClass;
    }



}
