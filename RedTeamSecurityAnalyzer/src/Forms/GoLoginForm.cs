using System.Collections.Concurrent;

namespace RedTeamSecurityAnalyzer.Forms;

public record GoLoginButton(string Pci, string ContactId, string DsCntCti, string CompanyId, IElementHandle ElementHandle);

[RegisterSingleton<ILoginForm>(Duplicate = DuplicateStrategy.Append, ServiceKey = RedTeamApplication.Go)]
public class GoLoginForm(IPuppeteerService puppeteerService) : ILoginForm
{
    public async Task<LoginResponse> LoginAsync(RedTeamSecurityAnalysisTestCase testCase, string username, string password, INotificationService notificationService)
    {
        try
        {
            var browser = await puppeteerService.GetBrowser(false, notificationService);
            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://go.redteam.com/manager/login.cfm");
            await notificationService.NotifyAsync($"Navigated to {page.Url.TextValue(true)}");
            await page.WaitForSelectorAsync("input[name='username']", new WaitForSelectorOptions() { Timeout = 0 });
            var usernameField = await page.QuerySelectorAsync("input[name='username']");
            if (usernameField != null) await usernameField.TypeAsync(username);

            var passwordField = await page.QuerySelectorAsync("input[type='password']");
            if (passwordField != null) await passwordField.TypeAsync(password);

            await page.ClickAsync("input[value='login']");
            await notificationService.NotifyAsync($"Posted login form");

            // Wait for navigation or response
            var response = await page.WaitForNavigationAsync(new NavigationOptions { Timeout = 0 });

            if (response == null)
            {
                await notificationService.NotifyAsync($"Login response was null".Error());
                return new LoginResponse(null, null);
            }

            if (!response.Ok)
            {
                return new LoginResponse(null, null);
            }

            var devlogins = await page.GoToAsync("https://go.redteam.com/manager/product/dev_logins.cfm");
            await notificationService.NotifyAsync($"Navigated  to {devlogins.Url.TextValue(true)}.");


            var loginButton = await GetGoLoginButton(page, testCase);
            notificationService.Information($"Found backdoor login button with pci {loginButton.Pci}, contactid {loginButton.ContactId}, dscntcti {loginButton.DsCntCti}, companyId {loginButton.CompanyId}");
            page = await ExecuteBackdoorLoginAsync(page, browser, loginButton, notificationService);
            return new(devlogins, page);
        }
        catch (Exception ex)
        {
            await notificationService.NotifyAsync($"Exception during login: {ex.Message}".Error());
            return new LoginResponse(null, null);
        }
    }

    private async Task<IPage?> ExecuteBackdoorLoginAsync(IPage page, IBrowser browser, GoLoginButton button, INotificationService notificationService)
    {
        var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await notificationService.NotifyAsync($"Begin Backdoor login with pci {button.Pci.Success()}, contactid {button.ContactId.Success()}, dscntcti {button.DsCntCti.Success()}, companyId {button.CompanyId.Success()}");
        var result = await page.EvaluateFunctionAsync(@"(pci, contactid, dscntcti, dsci) => {
            document.querySelector('#pci').value = pci;
            document.querySelector('#contactid').value = contactid;
            document.querySelector('#dscntcti').value = dscntcti;
            document.querySelector('#dsci').value = dsci;
            document.querySelector('#lgin').submit();
        }", button.Pci, button.ContactId, button.DsCntCti, button.CompanyId);

        var target = await browser.WaitForTargetAsync(x => x.Url.Contains("migrate"), new WaitForOptions { Timeout = 0 });

        var newPage = await target.PageAsync();

        if (newPage == null)
        {
            throw new Exception("Failed to open backdoor login page");
        }

        if (newPage!.Url.Contains("migrate"))
        {
            await notificationService.NotifyAsync("Skipping migration page".Warning());
            await newPage.GoToAsync("https://uatcode.uatapp.go.redteam.com?skip=y", new NavigationOptions { Timeout = 0 });
        }

        await notificationService.NotifyAsync($"Waiting for navigation to complete on new tab with url {newPage.Url}");
        return newPage;
    }

    private async Task<GoLoginButton> GetGoLoginButton(IPage page, RedTeamSecurityAnalysisTestCase testCase)
    {
        var backDoorUser = testCase.Properties;
        var buttons = await GetLoginButtons(page);
        return buttons.First(x => x.Pci == backDoorUser.DataAjax &&
        x.ContactId == backDoorUser.DataRel &&
        x.CompanyId == backDoorUser.DataId);
    }
    /// <summary>
    /// retrieves all GoLoginButton elements from the provided page.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    static async Task<IEnumerable<GoLoginButton>> GetLoginButtons(IPage page)
    {
        var buttons = await page.QuerySelectorAllAsync(".lobtn");
        var asyncEnumerable = buttons.ToAsyncEnumerable();

        var bag = new ConcurrentBag<GoLoginButton>();
        await foreach (var button in asyncEnumerable)
        {
            var dataAjax = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('data-ajax')");
            var contactId = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('data-rel')");
            var companyId = await button.EvaluateFunctionAsync<string>("el => el.getAttribute('data-id')");
            var goLoginButton = new GoLoginButton(dataAjax, contactId, contactId, companyId, button);
            bag.Add(goLoginButton);
        }
        return bag;
    }
}