using RedTeamSecurityAnalyzer.Extensions;

namespace RedTeamSecurityAnalyzer.TestExecution;

[RegisterSingleton<ITestRunner>(Duplicate = DuplicateStrategy.Append, ServiceKey = TestRunnerKeys.FormData)]
public class HttpFormPostTestRunner : AbstractTestRunner
{
    public override async Task<TestRunnerResponse> ExecuteAsync(RedTeamSecurityAnalysisTestCase test, RedTeamSecurityAnalysisRule rule, TestRunnerContext context)
    {
        var (application, notificationService, loginFormFactory, browser, upn, authenticatedPage) = context;

        var testCaseResponses = new List<TestCaseResponse>();


        var totalAsDouble = (double)rule.TestData.Count;
        var totalRules = 1 / totalAsDouble;

        var defaultResponse = new TestRunnerResponse(test, rule, new List<TestCaseResponse>());

        var page = await Authenticate(test, rule, context);
        await notificationService.NotifyAsync($"Starting analysis for rule: {rule.Name.ToString().Information()}");

        var response = await page.GoToAsync(test.BaseUrl);




        if (response != null && response.Ok)
        {


            foreach (var testData in rule.TestData)
            {
                var (urlPattern, description, successStatusCode, failureStatusCodes) = testData;
                var ruleName = rule.Name;

                var formDetails = test.PropertiesTo<FormDetails>();
                if (formDetails == null)
                {
                    return defaultResponse;
                }

                foreach (var key in formDetails.FormKeys.Keys)
                {
                    formDetails.FormKeys[key] = testData.Pattern;
                }




                var formPostResponse = await page.PostFormAsync(formDetails, notificationService);

                int responseCode = formPostResponse != null ? (int)formPostResponse.Status : 500;

                if (successStatusCode.Contains(responseCode))
                {
                    await notificationService.NotifyAsync($"URL Pattern: {urlPattern.Success()} Passed.");
                    testCaseResponses.Add(new(SecurityAnalysisStatus.Passed, rule, testData, $"Pattern '{urlPattern}' Succeeded"));
                }
                else if (failureStatusCodes.Contains(responseCode))
                {
                    await notificationService.NotifyAsync($"URL Pattern: {urlPattern.Error()} Failed.");
                    string? content = null;
                    if (response != null)
                    {
                        //content = await response.TextAsync();
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
                    await notificationService.NotifyAsync($"I don't know what to do with this response {response?.Status}.  Skipping");
                    testCaseResponses.Add(new(SecurityAnalysisStatus.Unknown, rule, testData, $"URL {response?.Status}", content));
                }

                notificationService.ProgressTask.Increment(totalRules);
            }
        }

        return new TestRunnerResponse(test, rule, testCaseResponses);
    }
}