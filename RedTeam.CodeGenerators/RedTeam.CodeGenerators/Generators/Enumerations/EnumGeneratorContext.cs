using Microsoft.CodeAnalysis;
using RedTeam.CodeGenerators.Models;
using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.CodeGenerators.Generators.Enumerations;
public record EnumGeneratorContext(
    EquatableArray<EnumRegistration>? EnumRegistrations = null,
    EquatableArray<Diagnostic>? Diagnostics = null) : GenerationContext(Diagnostics)
{
    public override bool Valid => base.Valid;
}
