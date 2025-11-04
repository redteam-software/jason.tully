namespace RedTeam.Extensions.CodeGenerators;

/// <summary>
/// metadata required to register a static class configuration method
/// </summary>
/// <param name="ClassName">The fully qualified name of the class containing the static registration method.</param>
/// <param name="MethodName">The name of the registration method to invoke.</param>
public record MethodRegistration(string ClassName, string MethodName);
