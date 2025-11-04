using Microsoft.CodeAnalysis;
using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.Extensions.CodeGenerators.Helpers;
static class AttributeHelper
{
    /// <summary>
    /// Determine if the current attribute is an static method registration attribute.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool IsMethodRegistrationAttribute(AttributeData attribute)
    {
        return attribute?.AttributeClass is
        {
            Name: AttributeTypeNames.CommandHandlerRegistrationShortName or
                  AttributeTypeNames.CommandHandlerRegistrationTypeName,
            ContainingNamespace:
            {
                Name: "Attributes",
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

    /// <summary>
    /// Determine if the current attribute is an command handler attribute.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool IsCommandHandlerRegistrationAttribute(AttributeData attribute)
    {
        return attribute?.AttributeClass is
        {
            Name: AttributeTypeNames.AddAddCommandHandlerShortName or
                  AttributeTypeNames.AddAddCommandHandlerTypeName,
            ContainingNamespace:
            {
                Name: "Attributes",
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
    /// <summary>
    /// Determine if the current attribute is an sub command handler attribute.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool IsSubCommandHandlerRegistrationAttribute(AttributeData attribute)
    {
        return attribute?.AttributeClass is
        {
            Name: AttributeTypeNames.AddAddSubCommandHandlerShortName or
                  AttributeTypeNames.AddAddSubCommandHandlerTypeName,
            ContainingNamespace:
            {
                Name: "Attributes",
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool IsSubCommandAttribute(AttributeData attribute)
    {
        return attribute?.AttributeClass is
        {
            Name: AttributeTypeNames.SubCommandShortName or
                  AttributeTypeNames.SubCommandTypeName,
            ContainingNamespace:
            {
                Name: "Attributes",
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
}

