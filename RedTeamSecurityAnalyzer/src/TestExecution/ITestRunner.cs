using RedTeamSecurityAnalyzer.Forms;

namespace RedTeamSecurityAnalyzer.TestExecution;

public record TestRunnerContext(
    RedTeamApplication Application,
    IProgressTaskNotificationService NotificationService,
    ILoginFormFactory LoginFormFactory,
    IBrowser Browser,
    Upn? Upn = null,
    IPage? AuthenticatedPage = null);

public interface ITestRunner
{
    Task<TestRunnerResponse> ExecuteAsync(RedTeamSecurityAnalysisTestCase test, RedTeamSecurityAnalysisRule rule, TestRunnerContext TestRunnerContext);

    Task<List<TestRunnerResponse>> ExecuteAsync(RedTeamSecurityAnalysisTestCase test, TestRunnerContext TestRunnerContext);
}

public interface ITestRunnerFactory
{
    ITestRunner GetTestRunner(string name);
}

[RegisterSingleton<ITestRunnerFactory>]
public record TestCaseResponse(SecurityAnalysisStatus Status, RedTeamSecurityAnalysisRule Rule, RedTeamSecurityAnalysisTestData TestData, string Message, string? ErrorMessage = null);

public record TestRunnerResponse(RedTeamSecurityAnalysisTestCase TestCase, RedTeamSecurityAnalysisRule Rule, List<TestCaseResponse> TestCaseResponses);