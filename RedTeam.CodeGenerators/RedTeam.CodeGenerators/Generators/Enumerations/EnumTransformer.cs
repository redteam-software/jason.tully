using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RedTeam.CodeGenerators.Generators.Enumerations;
public static class EnumTransformer
{
    public static EnumGeneratorContext? Create(GeneratorSyntaxContext context, Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax enumDeclarationSyntax, CancellationToken cancellationToken)
    {
        var diagnostics = new List<Diagnostic>();

        bool isEnum = context.Node is EnumDeclarationSyntax;

        if (!isEnum)
        {
            return null;
        }

        var enumDeclaration = context.Node as EnumDeclarationSyntax;
        var enumMemberDeclarations = enumDeclaration.Members
             .OfType<EnumMemberDeclarationSyntax>()
             .ToList();
        var data = new Dictionary<string, int>();
        foreach (var member in enumMemberDeclarations)
        {
            var s = context.SemanticModel.GetDeclaredSymbol(member, cancellationToken) as IFieldSymbol;
            if (s != null && s.HasConstantValue)
            {
                var numericValue = s.ConstantValue; // This is boxed (int, byte, etc.)
                data.Add(member.Identifier.Text, Convert.ToInt32(numericValue));
            }
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;

        if (symbol is null)
            return null;

        var attributes = symbol.GetAttributes();
        var enumRegistrations = new List<EnumRegistration>();

        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass?.ToDisplayString() == "RedTeam.CodeGenerators.Attributes.AutoMapAttribute")
            {
                //Members: symbol.GetMembers().Where(m => m.Kind == SymbolKind.Field).Select(f => f.Name).ToArray()
                // we found the attribute, we can stop looking


                enumRegistrations.Add(new EnumRegistration(

                        symbol.ContainingNamespace.ToDisplayString(),
                        symbol.Name,
                       data
                    ));

            }
        }

        return new EnumGeneratorContext(
                   EnumRegistrations: enumRegistrations.ToArray(),
                   Diagnostics: diagnostics.ToArray()
               );
    }
}
