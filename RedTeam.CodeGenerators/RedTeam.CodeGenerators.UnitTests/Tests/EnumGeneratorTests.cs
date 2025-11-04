using FluentAssertions;
using RedTeam.CodeGenerators.Generators.Enumerations;
using RedTeam.CodeGenerators.UnitTests.Utils;

namespace RedTeam.CodeGenerators.UnitTests.Tests;
public class EnumGeneratorTests
{
    [Fact]
    public void GetGeneratedOutput_EnumGenerator_Returns_GeneratedCode()
    {
        var source = """
            using RedTeam.CodeGenerators.Attributes;
            namespace RedTeam.CodeGenerators.UnitTests.Data;

            [AutoMap]
            public enum Persons
            {
                Alice,
                Bob,
                Charlie,
                Tim = 6
            }

            [AutoMap]
            public enum Cars
            {
                 Chevrolet = 1,
                 Ford = 2,
                 Mercedes = 3,
                 Tesla = 4,
                 Toyota = 5,
                 Subaru = 6
            }
            """;

        var (diagnostics, output) = GeneratorHelper.GetGeneratedOutput<EnumGenerator>(source, RequiredAssemblies.Assemblies);

        output.Should().NotBeNullOrEmpty();

    }
}
