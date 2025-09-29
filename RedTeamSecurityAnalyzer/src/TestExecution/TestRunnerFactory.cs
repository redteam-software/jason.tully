namespace RedTeamSecurityAnalyzer.TestExecution;

[RegisterSingleton<ITestRunnerFactory>]
public class TestRunnerFactory : ITestRunnerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TestRunnerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITestRunner GetTestRunner(string name)
    {
        return _serviceProvider.GetRequiredKeyedService<ITestRunner>(name);
    }
}