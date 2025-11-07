using RedTeamGoCli.Models.Projects;

namespace RedTeamGoCli.Models;

public class GoProjectEnvironmentConfiguration
{

    public GoProjectConfiguration Uat { get; set; } = null!;
    public GoProjectConfiguration Prod { get; set; } = null!;



}

public class GoProjectConfiguration
{

    public GrafanaCloudConfiguration GrafanaCloudConfiguration { get; set; } = null!;
    public PlatformConfiguration PlatformConfiguration { get; set; } = null!;
    public GoManager GoManager { get; set; } = null!;
    public GoColdFusion GoColdFusion { get; set; } = null!;
    public GoLaravel GoLaravel4 { get; set; } = null!;
    public GoLaravel GoLaravel5 { get; set; } = null!;
}