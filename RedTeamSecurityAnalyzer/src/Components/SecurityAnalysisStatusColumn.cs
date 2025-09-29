using Spectre.Console;
using Spectre.Console.Rendering;

namespace RedTeamSecurityAnalyzer.Components;

internal class SecurityAnalysisStatusColumn : ProgressColumn
{
    protected override bool NoWrap => true;

    /// <summary>
    /// Gets or sets the style of the remaining time text.
    /// </summary>
    public Style Style { get; set; } = Color.Blue;

    /// <inheritdoc/>
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var message = task.State.Get<SecurityAnalysisStatusMessage>(nameof(SecurityAnalysisStatusMessage));

        var total = message.FaiiledRules + message.PassedRules;

        var label = $"{message.FaiiledRules.ToString("N0").Error()}/{message.PassedRules.ToString("N0").Success()}";

        try
        {
            return new Markup(label);
        }
        catch
        {
            return new Text(label, Style ?? Style.Plain);
        }
    }

    /// <inheritdoc/>
    public override int? GetColumnWidth(RenderOptions options)
    {
        return 30;
    }
}