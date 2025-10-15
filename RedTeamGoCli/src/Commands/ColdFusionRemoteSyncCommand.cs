using RedTeamGoCli.Services;
using System.Diagnostics;

namespace RedTeamGoCli.Commands;

public record ColdFusionRemoteSyncParameters(
  [Option("user", Description = "The Cold Fusion FTP Server User")] string? user,
  [Option("password", Description = "The Cold Fusion FTP Server Password")] string? password,
  [Option("config", Description = "An optional configuration file that contains the user/password for the ftp server.  ")] string? configurationFile,
  [Option("batch", Description = "The number of file changes to batch before uploading. Default is 5")] int batch = 2,
  [Option("debounce", Description = "The time in seconds to wait before processing a batch. Default is 10 seconds")] int debounce = 3,
  [Option("host", Description = "The FTP server host ip address.")] string? host = null,
  [Option("remote-dir", Description = "The FTP server remote directory")] string? remoteDir = null,
  string? logLevel = "error") : CommandParameters(logLevel);

[CommandHandler("cf-sync", "Using the current project directory, starts a local file system monitor to synchronize changes between the local Cold Fusion Go project and the remotely deployed UAT instance.")]
public class ColdFusionRemoteSyncCommand : ICommand<ColdFusionRemoteSyncParameters>
{

    private readonly IFileSystemChangeMonitor _fileSystemChangeMonitor;
    private readonly IRemoteChangeSynchronizationService _remoteChangeSynchronizationService;

    public ColdFusionRemoteSyncCommand(
        IFileSystemChangeMonitor fileSystemChangeMonitor,
        IRemoteChangeSynchronizationService remoteChangeSynchronizationService)
    {

        _fileSystemChangeMonitor = fileSystemChangeMonitor;
        _remoteChangeSynchronizationService = remoteChangeSynchronizationService;
    }
    public async Task RunAsync(ColdFusionRemoteSyncParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {
        "cf-sync - Syncs local changes with remote UAT instance".WriteSubTitle(false);
        if (Debugger.IsAttached)
        {
            "Debugger is attached, setting current directory to 'D:\\Development\\RedTeam\\Go\\all-go-projects\\go-production-cf'".Warning().WriteLine();
            Environment.CurrentDirectory = @"D:\Development\RedTeam\Go\all-go-projects\go-production-cf";
        }


        args = ValidateParameters(args);


        if (string.IsNullOrEmpty(args.user) || string.IsNullOrEmpty(args.password))
        {
            "You must provide a user and password to connect to the Cold Fusion FTP Server.".Error().WriteLine();
            return;
        }

        if (!ValidateColdFusionProject(Environment.CurrentDirectory))
        {
            $"{Environment.CurrentDirectory.TextValue(true).Error()} is not a Cold Fusion Project.".WriteLine();
            return;
        }

        var table = new Table().Expand().HideHeaders().HideRowSeparators().NoBorder();


        table.AddColumns("Message", "Time");
        table.AddEmptyRow();

        await AnsiConsole.Live(table)
            .StartAsync(async ctx =>
            {
                var notificationService = new LiveTableNotificationService(ctx, table, 25);
                var parameters = new SynchronizationParameters(args.user, args.password, args.batch, args.debounce, args.host, args.remoteDir);
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

    private bool ValidateColdFusionProject(string directory)
    {
        var files = Directory.GetFiles(directory, "*.cfm");
        return files.Any();

    }

    private ColdFusionRemoteSyncParameters ValidateParameters(ColdFusionRemoteSyncParameters args)
    {

        //"C:\Users\Jason Tully\.secrets\rtgo.txt"

        var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var rtgoConfigPath = Path.Combine(userProfilePath, ".secrets", "rtgo.txt");

        if (string.IsNullOrEmpty(args.configurationFile) && File.Exists(rtgoConfigPath))
        {
            $"Loading configuration from {rtgoConfigPath.TextValue(true)}".WriteLine();
            args = args with { configurationFile = rtgoConfigPath };
        }


        if (!string.IsNullOrEmpty(args.configurationFile) && File.Exists(args.configurationFile))
        {
            var lines = File.ReadAllLines(args.configurationFile);
            foreach (var line in lines)
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim().ToLower();
                    var value = parts[1].Trim();
                    if (key == "user" && string.IsNullOrEmpty(args.user))
                    {
                        args = args with { user = value };
                    }
                    else if (key == "password" && string.IsNullOrEmpty(args.password))
                    {
                        args = args with { password = value };
                    }
                }
            }
        }


        return args;
    }
}



