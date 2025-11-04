namespace RedTeamGoCli.Models.Projects;
public record GoLaravel(string ProjectDirectory,
    string Description,
    string Name,
    string TargetBranch,
    string RemoteLogPath,
    string Host,
    string LogFilePrefix,
    string RemoteDirectory,
    string LogFileExtension = ".log"
    ) : IGoProject, IGoRemoteLogProject, IGoRemoteServiceProject
{
}
