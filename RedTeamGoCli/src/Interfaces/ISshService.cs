namespace RedTeamGoCli.Interfaces;

public interface ISshService
{
    public string GetRemoteLog(IGoRemoteLogProject project, bool format = true);

    public List<string> ListRemoteLogCommand(IGoRemoteLogProject project);
}