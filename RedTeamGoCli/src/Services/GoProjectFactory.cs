using System.Text.Json;

namespace RedTeamGoCli.Services;

[RegisterSingleton]
public class GoProjectFactory : IGoProjectFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GoProjectFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RegisterServices]
    public static IServiceCollection AddGoProjectFactory(IServiceCollection services)
    {
        var configurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".secrets", "rtgo.json");

        if (!Path.Exists(configurationPath))
        {
            throw new FileNotFoundException("Go configuration file not found.", configurationPath);
        }

        var json = File.ReadAllText(configurationPath);

        var configuration = JsonSerializer.Deserialize<GoProjectEnvironmentConfiguration>(json)!;

        if (configuration == null)
        {
            throw new InvalidOperationException("Failed to deserialize Go project configuration.");
        }


        services.AddKeyedSingleton("uat", (sp, key) => configuration.Uat);
        services.AddKeyedSingleton("prod", (sp, key) => configuration.Prod);

        RegisterForEnvironment(services, "uat", configuration.Uat);
        RegisterForEnvironment(services, "prod", configuration.Prod);

        return services;
    }
    private static void RegisterForEnvironment(IServiceCollection services, string env, GoProjectConfiguration configuration)
    {

        var keyGoManager = $"{env}-go-manager";
        var keyColdFusion = $"{env}-go-production-cf";
        var keyLaravel4 = $"{env}-go-production-v4";
        var keyLaravel5 = $"{env}-go-production-v5";


        services.AddKeyedSingleton<IGoFtpProject>(keyGoManager, configuration.GoManager)
                  .AddSingleton<IGoProject>(configuration.GoManager)
                  .AddKeyedSingleton<IGoRemoteServiceProject>(keyGoManager, configuration.GoManager);

        services.AddKeyedSingleton<IGoFtpProject>(keyColdFusion, configuration.GoColdFusion)
                   .AddSingleton<IGoProject>(configuration.GoColdFusion)
                   .AddKeyedSingleton<IGoRemoteServiceProject>(keyColdFusion, configuration.GoColdFusion);

        services.AddKeyedSingleton<IGoAutomaticPullRequestProject>(keyLaravel4, configuration.GoLaravel4)
                    .AddSingleton<IGoProject>(configuration.GoLaravel4)
                    .AddKeyedSingleton<IGoRemoteServiceProject>(keyLaravel4, configuration.GoLaravel4)
                    .AddKeyedSingleton<IGoRemoteLogProject>(keyLaravel4, configuration.GoLaravel4);

        services.AddKeyedSingleton<IGoAutomaticPullRequestProject>(keyLaravel5, configuration.GoLaravel5)
                   .AddSingleton<IGoProject>(configuration.GoLaravel5)
                   .AddKeyedSingleton<IGoRemoteServiceProject>(keyLaravel5, configuration.GoLaravel5)
                   .AddKeyedSingleton<IGoRemoteLogProject>(keyLaravel5, configuration.GoLaravel5);
    }

    public IGoProject? GetProjectFromDirectory(ApplicationEnvironment env, string projectDirectory)
    {
        return GetProjectFromDirectory<IGoProject>(env, projectDirectory);
    }

    public T? GetProjectFromDirectory<T>(ApplicationEnvironment env, string projectDirectory) where T : IGoProject
    {
        var dir = new DirectoryInfo(projectDirectory);

        var segments = dir.FullName.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Reverse();

        foreach (var segment in segments)
        {
            var project = _serviceProvider.GetKeyedService<T>($"{env.Value}-{segment}");
            if (project != null)
            {
                return project;
            }
        }

        return default;
    }
}