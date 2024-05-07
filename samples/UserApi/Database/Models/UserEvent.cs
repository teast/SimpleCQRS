namespace UserApi.Database.Models;

public record UserEvent
{
    public int UserId { get; set; }
    public int Version { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string EventType { get; set; } = default!;
    public CQRS.Events.UserEvent EventData { get; set; } = default!;
}

