namespace RedTeamGoCli.Models;

public record SynchronizationParameters(IGoRemoteServiceProject RemoteTargetProject, int BatchSize = 5, int DebounceSeconds = 10);
