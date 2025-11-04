namespace RedTeamGoCli.Models;

public record NotificationId(string Value)
{
    public static NotificationId System => new NotificationId("0");

    public static implicit operator NotificationId(string value)
    {
        return new NotificationId(value);
    }
    public static implicit operator NotificationId(Guid guid) => new NotificationId(guid.ToString());
}