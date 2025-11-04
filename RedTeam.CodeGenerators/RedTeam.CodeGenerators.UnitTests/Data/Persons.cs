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



public static class PersonsMapper
{
    /// <summary>
    ///Maps RedTeam.CodeGenerators.UnitTests.Data.Persons to an integer.
    /// </summary>
    public static int? ToInt(this global::RedTeam.CodeGenerators.UnitTests.Data.Persons value)
    {
        return value switch
        {
            global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Alice => 0,
            global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Bob => 1,
            global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Charlie => 2,
            global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Tim => 6,
            _ => null
        };
    }
    /// <summary>
    ///Maps a string to  RedTeam.CodeGenerators.UnitTests.Data.Persons.
    /// </summary>
    public static global::RedTeam.CodeGenerators.UnitTests.Data.Persons? FromString(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "alice" => global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Alice,
            "bob" => global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Bob,
            "charlie" => global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Charlie,
            "tim" => global::RedTeam.CodeGenerators.UnitTests.Data.Persons.Tim,
            _ => null
        };
    }
}
/// <summary>
/// Automapping extensions for Cars.
/// </summary>
public static class CarsMapper
{
    /// <summary>
    ///Maps RedTeam.CodeGenerators.UnitTests.Data.Cars to an integer.
    /// </summary>
    public static int? ToInt(this global::RedTeam.CodeGenerators.UnitTests.Data.Cars value)
    {
        return value switch
        {
            global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Chevrolet => 1,
            global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Ford => 2,
            global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Mercedes => 3,
            global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Tesla => 4,
            global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Toyota => 5,
            global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Subaru => 6,
            _ => null
        };
    }
    /// <summary>
    ///Maps a string to  RedTeam.CodeGenerators.UnitTests.Data.Cars.
    /// </summary>
    public static global::RedTeam.CodeGenerators.UnitTests.Data.Cars? FromString(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "chevrolet" => global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Chevrolet,
            "ford" => global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Ford,
            "mercedes" => global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Mercedes,
            "tesla" => global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Tesla,
            "toyota" => global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Toyota,
            "subaru" => global::RedTeam.CodeGenerators.UnitTests.Data.Cars.Subaru,
            _ => null
        };
    }
}
