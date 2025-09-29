namespace RedTeamSecurityAnalyzer.Forms;

public interface ILoginFormFactory
{
    public ILoginForm GetLoginForm(RedTeamApplication application);
}

[RegisterSingleton<ILoginFormFactory>]
public class LoginFormFactory : ILoginFormFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LoginFormFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILoginForm GetLoginForm(RedTeamApplication application)
    {
        return _serviceProvider.GetRequiredKeyedService<ILoginForm>(application);
    }
}