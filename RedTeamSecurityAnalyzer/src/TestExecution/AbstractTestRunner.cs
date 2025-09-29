namespace RedTeamSecurityAnalyzer.TestExecution;

public abstract class AbstractTestRunner : ITestRunner
{
    public async Task<List<TestRunnerResponse>> ExecuteAsync(RedTeamSecurityAnalysisTestCase test, TestRunnerContext TestRunnerContext)
    {
        var result = await Task.WhenAll(test.Rules.Select(x => ExecuteAsync(test, x, TestRunnerContext)));

        return result.ToList();
    }

    public abstract Task<TestRunnerResponse> ExecuteAsync(RedTeamSecurityAnalysisTestCase test, RedTeamSecurityAnalysisRule rule, TestRunnerContext TestRunnerContext);

    protected async Task<IPage> Authenticate(RedTeamSecurityAnalysisTestCase test, RedTeamSecurityAnalysisRule rule, TestRunnerContext testRunnerContext)
    {
        var (application, notificationService, loginFormFactory, browser, upn, authenticatedPage) = testRunnerContext;

        if (test.RequiresAuthentication || rule.RequiresAuthentication)
        {
            if (authenticatedPage != null)
            {
                return authenticatedPage;
            }

            try
            {

                var form = testRunnerContext.LoginFormFactory.GetLoginForm(testRunnerContext.Application);
                var response = await form.LoginAsync(test, testRunnerContext.Upn!.Username, testRunnerContext.Upn.Password, testRunnerContext.NotificationService);
                if (response.IsSuccess)
                {
                    testRunnerContext.NotificationService.Information($"Successfully authenticated to {test.BaseUrl} as {upn?.Username}.  Frames: {response.Page!.Frames.Count()}");

                    return response.Page!;
                }
            }
            catch (Exception ex)
            {
                notificationService.Exception(ex);

            }
        }

        return await browser.NewPageAsync();

    }

    protected virtual string BuildTestUrl(string baseUrl, string replacementToken, string pattern) => baseUrl.Replace(replacementToken, pattern);
}