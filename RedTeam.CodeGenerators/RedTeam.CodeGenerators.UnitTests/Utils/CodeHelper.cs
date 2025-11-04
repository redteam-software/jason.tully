using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RedTeam.CodeGenerators.UnitTests.Utils;
public static class CodeHelper
{
    public static SyntaxTree Parse(string generatedCode)
    {
        return CSharpSyntaxTree.ParseText(generatedCode);
    }
}
