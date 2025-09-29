using Cocona;
using Microsoft.Extensions.Configuration;
using RedTeamGoCrawler.Components;
using RedTeamGoCrawler.Extensions;
using RedTeamGoCrawler.Services;
using RedTeamGoCrawler.Services.NotificationServices;
using Spectre.Console;

namespace RedTeamGoCrawler.Commands;


public record CrawlParameters(
  [Option(Description = "pci")] string? pci = "370",
  [Option(Description = "company id")] string? companyId = "1",
  [Option(Description = "contact id")] string? contactId = "1",
  [Option(Description = "username")] string? username = null,
  [Option(Description = "password")] string? password = null,
  string? logLevel = "information") : CommandParameters(logLevel);

[CommandHandler("crawl", "Crawls the RedTeamGo application for security vulnerabilities.")]
public class CrawlCommand : ICommand<CrawlParameters>
{
    private readonly IApplicationBrowserService _goBrowserService;
    private readonly IConfiguration _configuration;

    public CrawlCommand(IApplicationBrowserService goBrowserService, IConfiguration configuration)
    {
        _goBrowserService = goBrowserService;
        _configuration = configuration;
    }

    public async Task RunAsync(CrawlParameters args, CommandContext commandContext, CancellationToken cancellationToken = default)
    {


        var splitLayout = new SplitLayout();

        await AnsiConsole.Live(splitLayout).StartAsync(async ctx =>
        {
            var leftPanelLogger = new LiveDataDisplayNotificationService(ctx, new Table());

            var upn = Upn.Create(args.username, args.password, _configuration);

            splitLayout.UpdateLeftPanel(leftPanelLogger.Renderable);
            splitLayout.UpdateRightPanel(new Text("Waiting for data..."));
            var page = await _goBrowserService.GoToPage("https://uatapp1.go.redteam.com/dashboard/index", upn, leftPanelLogger);

            var pageInfo = new PageInformation(page);
            await pageInfo.LoadAsync();

            splitLayout.UpdateRightPanel(pageInfo);


            ctx.Refresh();

            var menuLinks = await page.GetMenuLinksAsync();
            await leftPanelLogger.NotifyAsync($"Left menu links found: {menuLinks.Count().ToString().NumericValue()}");

            await foreach (var link in menuLinks.ToAsyncEnumerable())
            {
                await leftPanelLogger.NotifyAsync($"Navigating to {link.Name} {link.Url} {link.Id}");
                if (link.Name.Equals("Logout", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {

                    try
                    {
                        await page.ClickAsync($"a[data-id='{link.Id}']");

                        pageInfo = new PageInformation(page);
                        await pageInfo.LoadAsync();
                        ctx.Refresh();
                    }
                    catch (Exception ex)
                    {
                        await leftPanelLogger.NotifyAsync($"Failed to click link {link.Name} {ex.Message}.  {page.Url}".Error());
                    }
                }

            }





        });

    }
}