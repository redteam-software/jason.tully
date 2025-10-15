using FluentFTP;
using RedTeam.Extensions.Console.ExtensionMethods;
using System.Net;

namespace RedTeamGoCli;

//when you're logged in to. UATs folder is in mdrive>paskrcustomers>uatcode

public record GoColdFusionUploadResponse;
public record GoColdFusionChangeSet(IReadOnlyList<Change> Changes, string RemoteDirectory = "mdrive\\paskrcustomers\\uatcode");
public interface IGoColdFusionClient
{
    Task<GoColdFusionUploadResponse> UploadChangeSetToServer(GoColdFusionChangeSet goColdFusionChangeSet);

    Task<IEnumerable<string>> EnumerateDirectory(string remoteDirectory);
}

[RegisterSingleton<IGoColdFusionClient>]
class FtpGoColdFusionClient : IGoColdFusionClient
{

    private const string _pwdB64 = "QzNwMHIyZDIh";


    string GetPassword() => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(_pwdB64));
    private async Task<AsyncFtpClient> GetFtpClient()
    {
        // create an FTP client and specify the host, username and password
        // (delete the credentials to use the "anonymous" account)
        var pwd = GetPassword();
        var client = new AsyncFtpClient("10.0.0.4", "admin", pwd);
        await client.Connect();
        return client;
    }
    public async Task<IEnumerable<string>> EnumerateDirectory(string remoteDirectory)
    {

        var client = new AsyncFtpClient();
        client.Host = "10.0.0.4";
        var pwd = GetPassword();
        client.Credentials = new NetworkCredential("admin", pwd);
        try
        {
            $"Connecting to ${client.Host}".WriteLine();
            await client.Connect();
            $"Connection successful".Success().WriteLine();

            var files = await client.GetListing("mdrive\\paskrcustomers\\uatcode");

            return files.Select(x => x.FullName).ToList();
        }
        catch (Exception ex)
        {
            $"{ex.Message}".WriteLine();
        }

        return new List<string>();


    }

    public async Task<GoColdFusionUploadResponse> UploadChangeSetToServer(GoColdFusionChangeSet goColdFusionChangeSet)
    {





        var taskDescriptionColumn = new TaskDescriptionColumn();
        taskDescriptionColumn.Alignment = Justify.Left;
        await AnsiConsole.Progress().AutoRefresh(true)
             .Columns(new ProgressColumn[]
             {
                 taskDescriptionColumn,
                 new SpectreConsoleProgressTaskMessageColumn(),
                 new SpinnerColumn()
             }).StartAsync(async ctx =>
             {







                 var progressTasks = goColdFusionChangeSet.Changes.Select(async x =>
                 {
                     var t = ctx.AddTask(x.RelativePath);
                     t.IsIndeterminate = false;
                     t.MaxValue(100);
                     var client = new AsyncFtpClient();
                     client.Host = "10.0.0.4";
                     var pwd = GetPassword();
                     client.Credentials = new NetworkCredential("admin", pwd);
                     t.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"Connecting to ${client.Host}", 200, null));
                     await client.Connect();
                     t.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage($"Connection Successful".Success(), 200, null));
                     ctx.Refresh();

                     var uploadTask = client.UploadFile(x.FullPath, Path.Combine(goColdFusionChangeSet.RemoteDirectory, x.RelativePath),
                         FtpRemoteExists.OverwriteInPlace,
                         false,
                         FtpVerify.None,
                         progress: new ConsoleProgress(t, ctx));

                     t.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage(Path.Combine(goColdFusionChangeSet.RemoteDirectory, x.RelativePath), 200, null));

                     return uploadTask;
                 });
                 ctx.Refresh();

                 try
                 {


                     await Task.WhenAll(progressTasks);


                 }
                 catch (Exception ex)
                 {
                     $"{ex.Message}".WriteLine();
                 }





                 ctx.Refresh();







             });

        return new GoColdFusionUploadResponse();
    }

    internal class ConsoleProgress : IProgress<FtpProgress>
    {
        private readonly ProgressTask _progressTask;
        private readonly ProgressContext _progressContext;

        public ConsoleProgress(ProgressTask progressTask, ProgressContext progressContext)
        {
            _progressTask = progressTask;
            _progressContext = progressContext;
        }
        public void Report(FtpProgress value)
        {
            var task = _progressTask;


            task.Increment(value.Progress);


            if (value.Progress >= 100)
            {
                task.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage("Uploaded".Success(), 200, null));
                task.StopTask();
            }
            else
            {
                task.PostTaskMessage(new RedTeam.Extensions.Console.Models.ProgressTaskMessage("Uploading".Information(), 200, null));
            }
            _progressContext.Refresh();
        }
    }
}
