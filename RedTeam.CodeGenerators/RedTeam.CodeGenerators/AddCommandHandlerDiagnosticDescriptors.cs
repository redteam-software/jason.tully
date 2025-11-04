using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace RedTeam.Extensions.CodeGenerators;


[ExcludeFromCodeCoverage]
internal static class AddCommandHandlerDiagnosticDescriptors
{
    public static DiagnosticDescriptor InvalidCoconaAppParameter => new(
        id: "RedTeam100",
        title: "Invalid Method Parameter",
        messageFormat: "Invalid parameter {0} defined for method {1}. Should be Cocona.CoconaApp.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );



    public static DiagnosticDescriptor TooManyParameter => new(
       id: "RedTeam102",
       title: "Invalid Method Parameter",
       messageFormat: "Method {0} can have only 1 parameter of type Should be Cocona.CoconaApp. ",
       category: "Usage",
       defaultSeverity: DiagnosticSeverity.Error,
       isEnabledByDefault: true


   );

    public static DiagnosticDescriptor StaticMethod => new(
       id: "RedTeam103",
       title: "Invalid Access Modifier",
       messageFormat: "Method {0} must be static. ",
       category: "Usage",
       defaultSeverity: DiagnosticSeverity.Error,
       isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidClass => new(
      id: "RedTeam104",
      title: "Invalid Attribute Usage",
      messageFormat: "Method {0} must be defined within a command handler class.",
      category: "Usage",
      defaultSeverity: DiagnosticSeverity.Error,
      isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidAttributeUsage => new(
     id: "RedTeam105",
     title: "Invalid Attribute Usage",
     messageFormat: "Attribute must applied to  a command handler class.",
     category: "Usage",
     defaultSeverity: DiagnosticSeverity.Error,
     isEnabledByDefault: true);
}
