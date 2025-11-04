namespace RedTeamGoCli.Interfaces;
public interface IRedTeamPlatformService
{
    public Task<string?> GetAccessTokenAsync();

    public Task SignupAsync(string accessToken, string email, string applicationName);


}
