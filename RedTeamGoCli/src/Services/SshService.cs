using RedTeam.Extensions.Console.ShellInterop;
using System.Text;

namespace RedTeamGoCli.Services;

[RegisterSingleton]
[RegisterScoped<IRemoteService>(Duplicate = DuplicateStrategy.Append, ServiceKey = "go-production-v5")]
[RegisterScoped<IRemoteService>(Duplicate = DuplicateStrategy.Append, ServiceKey = "go-production-v4")]
public class SshService : ISshService, IRemoteService
{
    private readonly IShell _shell;

    public SshService(IShell shell)
    {
        _shell = shell;
    }

    public Task DeleteRemoteFile(string remotePath, IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public Task DownloadRemoteFile(string remotePath, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        var outputPath = Path.Combine(Environment.CurrentDirectory, destination);
        $"Downloading log {remotePath.TextValue()} from {remoteProject.Host.Success()}:{outputPath}".WriteLine();
        var response = _shell.Invoke($"scp {remoteProject.Host}:{remotePath} {outputPath}");
        if (!response.CompletedSuccessfully)
        {
            response.StandardError?.WriteLine();
        }

        return Task.CompletedTask;
    }

    public string GetRemoteLog(IGoRemoteLogProject project)
    {
        var logName = $"{project.LogFilePrefix}{DateTime.Now.Year}-{DateTime.Now.Month.ToString("D2")}-{DateTime.Now.Day.ToString("D2")}{project.LogFileExtension}";

        $"Downloading log {logName.TextValue()} from {project.Host.Success()}:{project.RemoteLogPath.TextValue()}".WriteLine();
        var response = _shell.Invoke($"scp {project.Host}:{project.RemoteLogPath}/{logName} {Path.Combine(Environment.CurrentDirectory, logName)}");
        if (!response.CompletedSuccessfully)
        {
            response.StandardError?.WriteLine();
        }
        return Path.Combine(Environment.CurrentDirectory, logName);
    }

    public Task<bool> Initialize(IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        $"Initializing SSH configuration from .ssh/config for host".WriteLine();
        return Task.FromResult(true);
    }

    public async Task<UploadStatus> UploadRemoteFile(SourceFile source, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        destination = destination.Replace("\\", "/");

        var correlationId = source.RelativePath.ToLower();

        await notificationService.NotifyAsync(new Notification(correlationId, $"Uploading {source.RelativePath.TextValue(true)}", "SSHUpload", DateTime.Now));
        try
        {
            var local = source.LocalPath;
            //sudo tar -cf - /protected/path/file.txt" | tar -xf -

            //first stage to temp location
            //then move to final location to avoid partial file issues
            var status = _shell.Invoke($"scp {local} {remoteProject.Host}:/tmp/{Path.GetFileName(source.LocalPath)}");
            if (status.CompletedSuccessfully)
            {
                status = _shell.Invoke($"ssh {remoteProject.Host}  sudo mv /tmp/{Path.GetFileName(source.LocalPath)} {destination}");
            }

            if (!status.CompletedSuccessfully)
            {
                await notificationService.NotifyAsync(new Notification(correlationId, "Failed To Upload File".Error(), "SSHUpload", DateTime.Now));
                return new UploadStatus(false, status.ToString());
            }
            else
            {
                await notificationService.NotifyAsync(new Notification(correlationId, $"File {source.RelativePath.Success()} Uploaded", "FileUpload", DateTime.Now));
                return new UploadStatus(true, "File Uploaded Successfully");
            }
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException;
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message).AppendLine(inner?.Message);
            await notificationService.NotifyAsync($"{sb.ToString().Error()} for file {destination}");
            return new UploadStatus(false, ex.Message);
        }
    }
}