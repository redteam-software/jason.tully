namespace RedTeamSecurityAnalyzer.Forms;

public record LoginResponse(IResponse? Response, IPage? Page)
{
    public bool IsSuccess => Response != null && Response.Status == System.Net.HttpStatusCode.OK && Page != null;
}

public interface ILoginForm
{
    Task<LoginResponse> LoginAsync(RedTeamSecurityAnalysisTestCase testCase, string username, string password, INotificationService notificationService);
}