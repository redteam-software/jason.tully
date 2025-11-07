using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    public Task<DownloadFileResult> DownloadRemoteFile(string remotePath, string destination, IGoRemoteServiceProject remoteProject, INotificationService notificationService, bool format = false)
    {
        DownloadFileResult? result = null;
        var outputPath = Path.Combine(Environment.CurrentDirectory, destination);
        $"Downloading log {remotePath.TextValue()} from {remoteProject.Host.Success()}:{outputPath}".WriteLine();
        var response = _shell.Invoke($"scp {remoteProject.Host}:{remotePath} {outputPath}");
        if (!response.CompletedSuccessfully)
        {
            response.StandardError?.WriteLine();
            return Task.FromResult(new DownloadFileResult(string.Empty, response.StandardError));
        }
        if (format)
        {
            FormatLogFile(outputPath);
        }
        return Task.FromResult(new DownloadFileResult(outputPath, response.StandardError));
    }

    public string GetRemoteLog(IGoRemoteLogProject project, bool format = true)
    {
        var logName = $"{project.LogFilePrefix}{DateTime.Now.Year}-{DateTime.Now.Month.ToString("D2")}-{DateTime.Now.Day.ToString("D2")}{project.LogFileExtension}";

        $"Downloading log {logName.TextValue()} from {project.Host.Success()}:{project.RemoteLogPath.TextValue()}".WriteLine();
        var response = _shell.Invoke($"scp {project.Host}:{project.RemoteLogPath}/{logName} {Path.Combine(Environment.CurrentDirectory, logName)}");
        if (!response.CompletedSuccessfully)
        {
            response.StandardError?.WriteLine();
        }
        var logFile = Path.Combine(Environment.CurrentDirectory, logName);
        if (format)
        {
            FormatLogFile(logFile);
        }
        return logFile;
    }

    public Task<bool> Initialize(IGoRemoteServiceProject remoteProject, INotificationService notificationService)
    {
        $"Initializing SSH configuration from .ssh/config for host".WriteLine();
        return Task.FromResult(true);
    }

    public List<string> ListRemoteLogCommand(IGoRemoteLogProject project)
    {
        var response = _shell.Invoke($"ssh {project.Host} ls {project.RemoteLogPath}");
        if (!response.CompletedSuccessfully)
        {
            response.StandardError?.WriteLine();
        }
        return response.Result?.ToList() ?? new List<string>();
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

    private void FormatLogFile(string path)
    {
        try
        {
            var arrayOfLogLines = new JArray();
            using (var reader = new StreamReader(path))
            {
                string? line = null;



                while ((line = reader.ReadLine()) != null)
                {
                    var jobject = JObject.Parse(line);

                    arrayOfLogLines.Add(jobject);

                }
            }

            var json = JsonConvert.SerializeObject(arrayOfLogLines, Formatting.Indented);

            File.WriteAllText(path, json);
        }
        catch
        {
            //do nothing
        }

    }
}