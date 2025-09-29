using Microsoft.Extensions.Configuration;
using RedTeamSecurityAnalyzer.Logging;
using Serilog;

"RedTeam".WriteApplicationLogo(new RedTeam.Extensions.Console.Fonts.TubesSmushed());

var logFile = "..\\..\\..\\RedTeamSecurityAnalyzer.log";
var htmlLogFile = "..\\..\\..\\RedTeamSecurityAnalyzer.html";
if (File.Exists(logFile))
{
    File.Delete(logFile);
}
if (File.Exists(htmlLogFile))
{
    File.Delete(htmlLogFile);
}
Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Information()
    .WriteTo.File(logFile)
    .WriteTo.File(new HtmlColorizingFormatter(), htmlLogFile)
   .CreateLogger();

CommandApp.CreateCommandAppBuilder(args, (ConfigurationSettings s) =>
{

    s.Host.UseSerilog(Log.Logger, dispose: true);
    s.Configuration.AddUserSecrets<Program>();
    s.Services.AddRedTeamSecurityAnalyzer();
})
    .RegisterRedTeamSecurityAnalyzerCommandHandlers()
    .Run();