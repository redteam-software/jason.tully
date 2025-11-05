namespace RedTeamGoCli.Models.Projects;
public record GoColdFusion(string ProjectDirectory, string Description,
    string Name, string RemoteDirectory, string Host, string User, string Password) :
    GoProject(ProjectDirectory, Description, Name), IGoFtpProject
{
}