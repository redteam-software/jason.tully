using Microsoft.Extensions.Configuration;
using RedTeam.CodeGenerators.Attributes;


namespace RedTeam.CodeGenerators.UnitTests.Utils;
internal class RequiredAssemblies
{
    public static readonly Type[] Assemblies = new[]
       {
            typeof(AutoMapAttribute),
            typeof(IConfiguration),
        };
}
