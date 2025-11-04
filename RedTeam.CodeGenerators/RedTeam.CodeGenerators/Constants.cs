using Microsoft.CodeAnalysis;

namespace RedTeam.Extensions.CodeGenerators;
internal static class Constants
{
    public static readonly SymbolDisplayFormat FullyQualifiedNullableFormat =
     SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
         SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
     );
}
