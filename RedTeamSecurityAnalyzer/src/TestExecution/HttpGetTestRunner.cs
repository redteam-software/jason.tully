namespace RedTeamSecurityAnalyzer.TestExecution;

[RegisterSingleton<ITestRunner>(Duplicate = DuplicateStrategy.Append, ServiceKey = TestRunnerKeys.HttpGet)]
public class HttpGetTestRunner : AbstractTestRunner
{
    public override async Task<TestRunnerResponse> ExecuteAsync(RedTeamSecurityAnalysisTestCase test, RedTeamSecurityAnalysisRule rule, TestRunnerContext TestRunnerContext)
    {
        var (application, notificationService, loginFormFactory, browser, upn, authenticatedPage) = TestRunnerContext;

        IPage page = await Authenticate(test, rule, TestRunnerContext);

        var testCaseResponses = new List<TestCaseResponse>();

        var totalAsDouble = (double)rule.TestData.Count;
        var totalRules = 1 / totalAsDouble;

        await notificationService.NotifyAsync($"Starting analysis for rule: {rule.Name.Information()}.   {rule.TestData.Count.NumericValue()} Test Cases.");

        foreach (var testData in rule.TestData)
        {
            var (urlPattern, description, successStatusCode, failureStatusCodes) = testData;
            var ruleName = rule.Name;
            var url = BuildTestUrl(test.BaseUrl, rule.ReplacementToken, testData.Pattern);

            var response = await page.GoToAsync(url);

            int responseCode = response != null ? (int)response.Status : 500;

            if (successStatusCode.Contains(responseCode))
            {
                notificationService.Information($"URL Pattern: {urlPattern} Passed with status {responseCode}");
                await notificationService.NotifyAsync($"URL Pattern: {urlPattern.Success()} Passed.");
                testCaseResponses.Add(new(SecurityAnalysisStatus.Passed, rule, testData, $"Pattern '{urlPattern}' Succeeded"));
            }
            else if (failureStatusCodes.Contains(responseCode))
            {
                notificationService.Information($"URL Pattern: {urlPattern} Failed with status {responseCode}");
                await notificationService.NotifyAsync($"URL Pattern: {urlPattern.Error()} Failed.");
                string? content = null;
                if (response != null)
                {
                    content = await response.TextAsync();
                }
                testCaseResponses.Add(new(SecurityAnalysisStatus.Failed, rule, testData, $"Pattern '{urlPattern}' Failed", content));
            }
            else
            {
                string? content = null;
                if (response != null)
                {
                    content = await response.TextAsync();
                }
                notificationService.Information($"I don't know what to do with this response {response?.Status}.  Skipping");
                await notificationService.NotifyAsync($"I don't know what to do with this response {response?.Status}.  Skipping");
                testCaseResponses.Add(new(SecurityAnalysisStatus.Unknown, rule, testData, $"URL '{url}' {response?.Status}", content));
            }

            notificationService.ProgressTask.Increment(totalRules);
        }

        if (testCaseResponses.All(x => x.Status == SecurityAnalysisStatus.Passed))
        {
            await notificationService.NotifyAsync($"All Test Cases Passed!".Success());
        }
        else
        {
            var failed = testCaseResponses.Count(x => x.Status == SecurityAnalysisStatus.Failed);
            var passed = testCaseResponses.Count(x => x.Status == SecurityAnalysisStatus.Passed);
            var notRun = testCaseResponses.Count(x => x.Status == SecurityAnalysisStatus.NotRun);
            var unknown = testCaseResponses.Count(x => x.Status == SecurityAnalysisStatus.Unknown);
            var total = testCaseResponses.Count;

            var summary = $"Total Test Cases: {total.NumericValue()}, Passed: {passed.ToString("N0").Success()}, Failed: {failed.ToString("N0").Error()}, Not Run: {notRun.ToString("N0").Information()}, Unknown: {unknown.ToString("N0").Warning()}";
            notificationService.Information($"Total Test Cases: {total}, Passed: {passed.ToString("N0")}, Failed: {failed.ToString("N0")}, Not Run: {notRun.ToString("N0")}, Unknown: {unknown.ToString("N0")}");
            await notificationService.NotifyAsync(summary);
        }

        return new TestRunnerResponse(test, rule, testCaseResponses);
    }
}