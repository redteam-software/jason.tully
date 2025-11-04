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

        var configuration = JsonSerializer.Deserialize<GoProjectConfiguration>(json)!;

        //var goManager = new GoManager("go-manager", "Go Cold Fusion Manager Project", "go-manager", "PASKR.COM", "", "", "");
        //var goProductionCf = new GoColdFusion("go-production-cf", "Go Cold Fusion  Project", "go-production-cf", "mdrive\\paskrcustomers\\uatcode", "", "", "");
        // var laravel4 = new GoLaravel("go-production-v4", "Laravel 4 Project", "laravel4", "uatcode_v3", "/var/www/html/uatapp1.go.redteam.com/current/app/storage/logs", "gouat4", "laravel-");
        //var laravel5 = new GoLaravel("go-production-v5", "Laravel 5 Project", "laravel5", "uat", "/var/www/html/uatapp.go.redteam.com/current/storage/logs/", "gouat5", "laravel-");

        services.AddKeyedSingleton<IGoFtpProject>("go-manager", configuration.GoManager)
                   .AddSingleton<IGoProject>(configuration.GoManager)
                   .AddKeyedSingleton<IGoRemoteServiceProject>("go-manager", configuration.GoManager);
        services.AddKeyedSingleton<IGoFtpProject>("go-production-cf", configuration.GoColdFusion)
                   .AddSingleton<IGoProject>(configuration.GoColdFusion)
                   .AddKeyedSingleton<IGoRemoteServiceProject>("go-production-cf", configuration.GoColdFusion);
        services.AddKeyedSingleton<IGoAutomaticPullRequestProject>("go-production-v4", configuration.GoLaravel4)
                    .AddSingleton<IGoProject>(configuration.GoLaravel4)
                    .AddKeyedSingleton<IGoRemoteServiceProject>("go-production-v4", configuration.GoLaravel4)
                    .AddKeyedSingleton<IGoRemoteLogProject>("go-production-v4", configuration.GoLaravel4);
        services.AddKeyedSingleton<IGoAutomaticPullRequestProject>("go-production-v5", configuration.GoLaravel5)
                   .AddSingleton<IGoProject>(configuration.GoLaravel5)
                   .AddKeyedSingleton<IGoRemoteServiceProject>("go-production-v5", configuration.GoLaravel5)
                   .AddKeyedSingleton<IGoRemoteLogProject>("go-production-v5", configuration.GoLaravel5);

        services.AddSingleton(configuration);

        return services;

    }
    public IGoProject? GetProjectFromDirectory(string projectDirectory)
    {
        return GetProjectFromDirectory<IGoProject>(projectDirectory);
    }
    public T? GetProjectFromDirectory<T>(string projectDirectory) where T : IGoProject
    {
        var dir = new DirectoryInfo(projectDirectory);

        var segments = dir.FullName.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Reverse();

        foreach (var segment in segments)
        {
            var project = _serviceProvider.GetKeyedService<T>(segment);
            if (project != null)
            {
                return project;
            }
        }

        return default;

    }
}
