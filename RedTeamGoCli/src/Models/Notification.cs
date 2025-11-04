namespace RedTeamGoCli.Models;

public record Notification(NotificationId Id, string Message, string Category = "System", DateTime? EventTime = null, bool ReplaceMessage = true)
{
}