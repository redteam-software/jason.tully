namespace RedTeamGoCli.Commands;

public record GoRemoteSyncParameters(
  [Option("batch", Description = "The number of file changes to batch before uploading. Default is 5")] int batch = 2,
  [Option("debounce", Description = "The time in seconds to wait before processing a batch. Default is 10 seconds")] int debounce = 3,
   [Option(Common.Path, Description = Common.PathDescription)] string? path = null,
  string? logLevel = "error") : CommandParameters(logLevel);

/// <summary>
/// Monitors the local file system for changes and synchronizes them in real-time with a remote UAT instance.
/// Uses batching and debouncing to optimize network operations. Displays live progress in a table format
/// showing sync activity and timestamps. Requires a valid Go Project with remote service configuration.
/// </summary>
[SubCommand(SubCommandDeploy.SubCommandName, SubCommandDeploy.SubCommandDescription)]
[SubCommandHandler(
    SubCommandDeploy.SubCommandName,
    SubCommandDeploy.CommandGoSync.CommandName,
   SubCommandDeploy.CommandGoSync.CommandDescription)]
public class GoRemoteSyncCommand : ICommand<GoRemoteSyncParameters>
{
    private readonly IFileSystemChangeMonitor _fileSystemChangeMonitor;
    private readonly IRemoteChangeSynchronizationService _remoteChangeSynchronizationService;
    private readonly IGoProjectFactory _goProjectFactory;

    public GoRemoteSyncCommand(
        IFileSystemChangeMonitor fileSystemChangeMonitor,
        IRemoteChangeSynchronizationService remoteChangeSynchronizationService,
        IGoProjectFactory goProjectFactory)
    {
        _fileSystemChangeMonitor = fileSystemChangeMonitor;
        _remoteChangeSynchronizationService = remoteChangeSynchronizationService;
        _goProjectFactory = goProjectFactory;
    }

    public async Task RunAsync(GoRemoteSyncParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        SubCommandDeploy.CommandGoSync.Format().WriteSubTitle(false);
        if (!string.IsNullOrEmpty(args.path) && Directory.Exists(args.path))
        {
            Environment.CurrentDirectory = args.path;
        }

        var project = _goProjectFactory.GetProjectFromDirectory<IGoRemoteServiceProject>(Environment.CurrentDirectory);
        if (project == null)
        {
            $"{Environment.CurrentDirectory.TextValue(true).Error()} is not a valid Go Project".WriteLine();
            return;
        }

        var table = new Table().Expand().HideHeaders().HideRowSeparators().NoBorder();

        $"Using Remote Directory {project.RemoteDirectory.TextValue(true)}".WriteLine();

        table.AddColumns("Message", "Time");
        // table.AddColumn("");
        table.AddEmptyRow();

        await AnsiConsole.Live(table)
            .StartAsync(async ctx =>
            {
                var notificationService = new LiveTableNotificationService(ctx, table, 25);
                var parameters = new SynchronizationParameters(project, args.batch, args.debounce);
                try
                {
                    await Task.WhenAll(
                        _fileSystemChangeMonitor.MonitorAsync(Environment.CurrentDirectory, notificationService, cancellationToken),
                        _remoteChangeSynchronizationService.StartAsync(parameters, notificationService, cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    //swallow it.
                }
            });
    }
}