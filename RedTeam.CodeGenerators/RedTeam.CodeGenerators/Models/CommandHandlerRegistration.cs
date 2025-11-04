using RedTeam.Extensions.CodeGenerators.Models;

namespace RedTeam.Extensions.CodeGenerators;

/// <summary>
/// Models metadata required to register an a cocona command handler.
/// </summary>
/// <param name="CommandHandlerClassName">The fully qualified name of the event handler class</param>
/// <param name="CommandParametersClassName">The fully qualified name of the message type consumed by this handler</param>
/// <param name="CommandName">The name of the registration method defined in RedTeam.Extensions.Lib.  This should match the attribute name</param>
/// <param name="CommandDescription">The name of the registration method defined in RedTeam.Extensions.Lib.  This should match the attribute name</param>
/// <param name="RegistrationMethodName">The name of the registration method defined in RedTeam.Extensions.Lib.  This should match the attribute name</param>

public record CommandHandlerRegistration(
string CommandHandlerClassName,
string CommandParametersClassName,
string CommandName,
string CommandDescription,
string RegistrationMethodName) : Registration(CommandHandlerClassName, CommandParametersClassName, RegistrationMethodName);

/// <summary>
/// 
/// </summary>
/// <param name="ParentCommandName"></param>
/// <param name="CommandHandlerClassName"></param>
/// <param name="CommandParametersClassName"></param>
/// <param name="CommandName"></param>
/// <param name="CommandDescription"></param>
/// <param name="RegistrationMethodName"></param>

public record SubCommandHandlerRegistration(
string ParentCommandName,
string CommandHandlerClassName,
string CommandParametersClassName,
string CommandName,
string CommandDescription,
string RegistrationMethodName) : CommandHandlerRegistration(CommandHandlerClassName, CommandParametersClassName, CommandName, CommandDescription, RegistrationMethodName);

public record SubCommandRegistration(
string CommandHandlerClassName,
string CommandName,
string CommandDescription,
string? ParentCommand = null);
