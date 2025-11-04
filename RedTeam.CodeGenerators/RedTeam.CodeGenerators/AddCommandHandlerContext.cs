using Microsoft.CodeAnalysis;
using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.Extensions.CodeGenerators;

/// <summary>
/// A context object that contains all collected metadata from source code.
/// </summary>
/// <param name="Diagnostics"></param>
/// <param name="CommandHandlerRegistrations"></param>
/// <param name="MethodRegistrations"></param>
public record AddCommandHandlerContext(
    EquatableArray<Diagnostic>? Diagnostics = null,
    EquatableArray<CommandHandlerRegistration>? CommandHandlerRegistrations = null,
    EquatableArray<SubCommandHandlerRegistration>? SubCommandHandlerRegistrations = null,
    EquatableArray<SubCommandRegistration>? SubCommandRegistrations = null,
    EquatableArray<MethodRegistration>? MethodRegistrations = null
)
{
    /// <summary>
    /// if any array contains data, we consider the context valid.
    /// </summary>
    public bool Valid =>
           CommandHandlerRegistrations?.Any() == true
        || SubCommandHandlerRegistrations?.Any() == true
        || MethodRegistrations?.Any() == true
        || SubCommandRegistrations?.Any() == true;
}
