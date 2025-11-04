namespace RedTeamGoCli.Interfaces;

public interface IGoColdFusionClient
{
    Task<GoColdFusionUploadResponse> UploadChangeSetToServer(GoColdFusionChangeSet goColdFusionChangeSet);

    Task<IEnumerable<string>> EnumerateDirectory(string remoteDirectory);
}
