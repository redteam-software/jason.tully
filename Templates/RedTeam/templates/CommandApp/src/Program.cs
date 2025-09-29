"RedTeamCommandApp".WriteApplicationLogo();


CommandApp.CreateCommandAppBuilder(args, s =>
{
    s.AddRedTeamCommandApp();
})
    .RegisterRedTeamCommandAppCommandHandlers()
    .Run();