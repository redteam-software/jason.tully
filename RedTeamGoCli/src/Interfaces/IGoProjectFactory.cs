namespace RedTeamGoCli.Interfaces;

public interface IGoProjectFactory
{
    public IGoProject? GetProjectFromDirectory(ApplicationEnvironment env, string projectDirectory);

    public T? GetProjectFromDirectory<T>(ApplicationEnvironment env, string projectDirectory) where T : IGoProject;
}