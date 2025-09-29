using RedTeamSecurityAnalyzer.Forms;
using static RedTeamSecurityAnalyzer.Services.NotificationServiceExtensions;

namespace RedTeamSecurityAnalyzer.Services;

public enum RedTeamApplication
{
    Go,
    Flex,
    Lens,
}

public static class RedTeamApplicationUtil
{
    public static RedTeamApplication Parse(string name)
    {
        return name.ToLower() switch
        {
            "go" => RedTeamApplication.Go,
            "flex" => RedTeamApplication.Flex,
            "lens" => RedTeamApplication.Lens,
            _ => RedTeamApplication.Go
        };
    }
}

/// <summary>
/// An abstraction for programatically browsing RedTeam applications.
/// </summary>
public interface IApplicationBrowserService : IAsyncDisposable
{
    /// <summary>
    ///  Whether or not the user is currently logged in.
    /// </summary>
    public bool IsLoggedIn { get; }

    /// <summary>
    /// Gets the underlying browser instance from puppeteer.  The browser instance is shared across the application.
    /// </summary>
    /// <returns></returns>
    Task<IBrowser> GetBrowser();

    /// <summary>
    /// If the use not currently logged in, this will throw an exception.   You must invoke Login first if you don't pass a login form.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="Upn"></param>
    /// <param name="notificationService"></param>
    /// <param name="loginForm">pass an option login form to combine login </param>
    /// <returns></returns>
    public Task<IPage> GoSecurePage(string url, Upn Upn, INotificationService notificationService, ILoginForm? loginForm = null);

    /// <summary>
    /// Anonymous page access to a URL.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="Upn"></param>
    /// <param name="notificationService"></param>
    /// <returns></returns>
    public Task<IPage> GoToPage(string url, Upn Upn, INotificationService notificationService);

    /// <summary>
    /// Logs in a user to the application using the specified browser and page context.
    /// </summary>
    /// <remarks>This method performs the login operation within the provided browser and page context. Ensure
    /// that the  <paramref name="browser"/> and <paramref name="page"/> instances are valid and properly initialized
    /// before  calling this method. The <paramref name="notificationService"/> may be used to provide feedback to the
    /// user  during the login process.</remarks>
    /// <param name="browser">The browser instance to use for the login operation.</param>
    /// <param name="page">The current page context where the login process will be performed.</param>
    /// <param name="upn">The user's unique principal name (UPN) used for authentication.</param>
    /// <param name="notificationService">The notification service used to display messages or alerts during the login process.</param>
    /// <returns>A task that represents the asynchronous login operation. The task result contains the updated page context
    /// after the login is completed, or <see langword="null"/> if the login fails.</returns>
    public Task<IPage?> Login(ILoginForm loginForm, Upn upn, INotificationService notificationService);
}

[RegisterSingleton<IApplicationBrowserService>]
public class ApplicationBrowserService : IApplicationBrowserService
{
    private readonly IPuppeteerService _puppeteerService;
    private bool _loggedIn = false;
    private string _loggedInUrl = string.Empty;

    public ApplicationBrowserService(IPuppeteerService puppeteerService)
    {
        _puppeteerService = puppeteerService;
    }

    public bool IsLoggedIn => _loggedIn;

    public async ValueTask DisposeAsync()
    {
        await _puppeteerService.DisposeAsync();
    }

    public async Task<IBrowser> GetBrowser()
    {
        return await _puppeteerService.GetBrowser(false, EmptyNotificationService.Default);
    }

    public async Task<IPage> GoSecurePage(string url, Upn Upn, INotificationService notificationService, ILoginForm? loginForm = null)
    {
        IPage? loggedInPage = null;
        if (loginForm != null)
        {
            loggedInPage = await Login(loginForm, Upn, notificationService);
        }
        else if (!_loggedIn)
        {
            throw new InvalidOperationException("Not logged in.  Call login first.");
        }
        else
        {
            loggedInPage = await _puppeteerService.GetLoginPage(_loggedInUrl);
        }

        var currentPath = new Uri(_loggedInUrl);
        var targetpAth = new Uri(url);

        if (currentPath.Segments.Last() == targetpAth.Segments.Last())
        {
            return loggedInPage!;
        }
        else
        {
            var page = await GoToPage(loggedInPage, url, Upn, notificationService);
            return page!;
        }
    }

    public async Task<IPage> GoToPage(string url, Upn upn, INotificationService notificationService)
    {
        var page = await GoToPage(default, url, upn, notificationService);
        return page!;
    }

    public async Task<IPage?> Login(ILoginForm loginForm, Upn upn, INotificationService notificationService)
    {
        var (username, password) = upn;
        await notificationService.NotifyAsync($"Logging in with user {username!.Success()}");

        var loginResponse = await loginForm.LoginAsync(username!, password!, notificationService);

        if (loginResponse.IsSuccess)
        {
            _loggedIn = true;
            _loggedInUrl = loginResponse.Page!.Url;
            return loginResponse.Page;
        }
        else
        {
            _loggedIn = false;
            return default;
        }
    }

    private async Task<IPage?> GoToPage(IPage? page, string url, Upn upn, INotificationService notificationService)
    {
        var browser = await _puppeteerService.GetBrowser(false, notificationService);
        page = page ?? await browser.NewPageAsync();
        await page.GoToAsync(url, new NavigationOptions { Timeout = 0 });
        await notificationService.NotifyAsync($"Loaded Page {page.Url.TextValue(true)}");
        return page;
    }
}