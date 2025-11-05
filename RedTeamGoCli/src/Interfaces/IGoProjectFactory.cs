namespace RedTeamGoCli.Interfaces;

public interface IGoProjectFactory
{
    public IGoProject? GetProjectFromDirectory(string projectDirectory);

    public T? GetProjectFromDirectory<T>(string projectDirectory) where T : IGoProject;
}