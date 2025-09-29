using static RedTeamSecurityAnalyzer.Services.NotificationServiceExtensions;

namespace RedTeamSecurityAnalyzer.Services;

public interface IPuppeteerService : IAsyncDisposable
{
    /// <summary>
    /// Retrieves an instance of the browser, optionally clearing the locally cached browser downloads.
    /// </summary>
    /// <param name="clearDownloads">A value indicating whether to clear the locally cached browser downloads.  <see langword="true"/> to clear the
    /// locally cached browser downloads; otherwise, <see langword="false"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an instance of the browser.</returns>
    Task<IBrowser> GetBrowser(bool clearDownloads, INotificationService notificationService);
}

public static class PuppeteerServiceExtensions
{
    /// <summary>
    /// This is the browser page that made the initial request to the login page.  Use this as a starting point for authenticated requests.
    /// </summary>
    /// <param name="puppeteerService"></param>
    /// <param name="clearDownloads"></param>
    /// <param name="notificationService"></param>
    /// <returns></returns>
    public static async Task<IPage?> GetLoginPage(this IPuppeteerService puppeteerService, string url)
    {
        var browser = await puppeteerService.GetBrowser(false, EmptyNotificationService.Default);
        var pages = await browser.PagesAsync();
        if (pages.Any())
        {
            return pages.FirstOrDefault(x => x.Url == url);
        }
        return default;
    }
}

[RegisterSingleton<IPuppeteerService>]
public class PuppeteerService : IPuppeteerService
{
    private const string _applicationName = "redteamgocrawler";

    private IBrowser? _browser;

    public void Dispose()
    {
        if (_browser != null)
        {
            _browser.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            var pages = await _browser.PagesAsync();

            foreach (var page in pages)
            {
                try
                {
                    await page.CloseAsync();
                    await page.DisposeAsync();
                }
                catch { }
            }

            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
    }

    public async Task<IBrowser> GetBrowser(bool clearDownloads, INotificationService notificationService)
    {
        if (_browser != null)
        {
            return _browser;
        }
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), _applicationName);
        if (!Directory.Exists(path))
        {
            await notificationService.NotifyAsync($"Creating {_applicationName} folder under user profile");

            Directory.CreateDirectory(path);
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var downloadPath = Path.Combine(path, "CustomChromium");
        await notificationService.NotifyAsync($"Attemping to set up puppeteer to use Chromium found under directory {downloadPath.TextValue(true)} ");

        if (!Directory.Exists(downloadPath))
        {
            await notificationService.NotifyAsync("Creating Directory " + path.TextValue(true));
            Directory.CreateDirectory(downloadPath);
            await notificationService.NotifyAsync("Downloading Chromium");
        }
        else if (clearDownloads)
        {
            try
            {
                Directory.Delete(downloadPath, true);
            }
            catch
            {
            }
            finally
            {
                if (!Directory.Exists(downloadPath))
                {
                    await notificationService.NotifyAsync("Creating Directory " + path);
                    Directory.CreateDirectory(downloadPath);
                    await notificationService.NotifyAsync("Downloading Chromium");
                }
            }

            await notificationService.NotifyAsync("Downloading Chromium");
        }
        var browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
        var browserFetcher = new BrowserFetcher(browserFetcherOptions);
        await browserFetcher.DownloadAsync();

        var browser = browserFetcher.GetInstalledBrowsers().First();
        var executablePath = browserFetcher.GetExecutablePath(browser.BuildId);

        await notificationService.NotifyAsync($"Launching Puppeteer From Custom Path {executablePath.TextValue(true)} ");

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            ExecutablePath = executablePath,
            Headless = true,
            Args = ["--no-sandbox", "--disable-web-security"]
        });

        return _browser;
    }
}