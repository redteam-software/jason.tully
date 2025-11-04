namespace RedTeam.Extensions.CodeGenerators.Models;

public class AttributeTypeNames
{
    public const string AttributeNamespace = "RedTeam.Extensions.Console.CodeGenerators.Attributes";

    public const string AddAddCommandHandlerShortName = "CommandHandler";
    public const string AddAddCommandHandlerTypeName = $"{AddAddCommandHandlerShortName}Attribute";
    public const string AddAddCommandHandlerFullName = $"{AttributeNamespace}.{AddAddCommandHandlerTypeName}";


    public const string CommandHandlerRegistrationShortName = "CommandHandlerRegistration";
    public const string CommandHandlerRegistrationTypeName = $"{CommandHandlerRegistrationShortName}Attribute";
    public const string CommandHandlerRegistrationFullName = $"{AttributeNamespace}.{CommandHandlerRegistrationTypeName}";

    public const string AddAddSubCommandHandlerShortName = "SubCommandHandler";
    public const string AddAddSubCommandHandlerTypeName = $"{AddAddSubCommandHandlerShortName}Attribute";
    public const string AddAddSubCommandHandlerFullName = $"{AttributeNamespace}.{AddAddSubCommandHandlerTypeName}";

    public const string SubCommandShortName = "SubCommandAttribute";
    public const string SubCommandTypeName = $"{SubCommandShortName}Attribute";
    public const string SubCommandFullName = $"{AttributeNamespace}.{SubCommandTypeName}";


}
