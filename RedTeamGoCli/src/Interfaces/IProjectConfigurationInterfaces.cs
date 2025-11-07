namespace RedTeamGoCli.Interfaces;
/// <summary>
/// Base interface for Go project settings.
/// </summary>
public interface IGoProject
{
    string ProjectDirectory { get; }
    string Description { get; }
    string Name { get; }
}

public interface IGoAutomaticPullRequestProject : IGoProject
{
    string TargetBranch { get; }
}

public interface IGoRemoteLogProject : IGoRemoteServiceProject
{
    string RemoteLogPath { get; }

    string LogFilePrefix { get; }

    string LogFileExtension { get; }
}

public interface IGoRemoteServiceProject : IGoProject
{
    string Host { get; }
    string RemoteDirectory { get; }
}

/// <summary>
/// Provides FTP synchronization settings for Go projects.
/// </summary>
public interface IGoFtpProject : IGoRemoteServiceProject
{
    string User { get; }
    string Password { get; }
}