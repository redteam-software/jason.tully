namespace RedTeamGoCli.Models.Projects;
public record GoProject(string ProjectDirectory, string Description, string Name) : IGoProject
{
}
