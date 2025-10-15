
"RedTeam Go  CLI".WriteApplicationLogo(new RedTeam.Extensions.Console.Fonts.StraightFont());
$"A set of tools making Go development easier.".Colorize(ThemeColors.Title, new TextDecorations(true, true, false, false)).WriteLine();



CommandApp.CreateCommandAppBuilder(args, s =>
{
    s.AddRedTeamGoCli();
})
    .RegisterRedTeamGoCliCommandHandlers()
    .Run();