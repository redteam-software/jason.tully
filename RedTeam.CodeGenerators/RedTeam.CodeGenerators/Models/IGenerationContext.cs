using Microsoft.CodeAnalysis;
using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.CodeGenerators.Models;
public record GenerationContext(EquatableArray<Diagnostic>? Diagnostics = null)
{
    public virtual bool Valid => Diagnostics?.Any() != true;
}
