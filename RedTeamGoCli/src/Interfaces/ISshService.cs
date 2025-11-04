namespace RedTeamGoCli.Interfaces;
public interface ISshService
{
    public string GetRemoteLog(IGoRemoteLogProject project);
}
