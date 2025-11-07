namespace RedTeamGoCli.Models.Projects;

public record GoRemoteServiceProject(string Host, string RemoteDirectory, string ProjectDirectory, string Description, string Name) : IGoRemoteServiceProject;
public record GoRemoteLogProject(string Host, string RemoteDirectory, string RemoteLogPath, string LogFilePrefix, string LogFileExtension,
     string ProjectDirectory, string Description, string Name) :
    GoRemoteServiceProject(Host, RemoteDirectory, ProjectDirectory, Description, Name), IGoRemoteLogProject
{
}
