using Serilog.Events;
using Serilog.Formatting;

namespace RedTeamSecurityAnalyzer.Logging;
public class HtmlColorizingFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        string colorClass;
        switch (logEvent.Level)
        {
            case LogEventLevel.Information:
                colorClass = "info-log";
                break;
            case LogEventLevel.Warning:
                colorClass = "warning-log";
                break;
            case LogEventLevel.Error:
            case LogEventLevel.Fatal:
                colorClass = "error-log";
                break;
            default:
                colorClass = "default-log";
                break;
        }

        var renderedMessage = logEvent.RenderMessage();



        var logMessage = $"""
            <div>
              <span class='time-stamp'>[{logEvent.Timestamp}] </span><span class='{colorClass}'>({logEvent.Level}) </span><span class='log-message text-sm''> {renderedMessage}</span>
            </div>
            """;

        output.WriteLine(logMessage);


    }


}
