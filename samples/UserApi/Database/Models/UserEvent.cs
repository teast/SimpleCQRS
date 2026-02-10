using Teast.SimpleCQRS;

namespace UserApi.Database.Models;

public record UserEvent : EventRecord<CQRS.Events.UserEvent>
{
    public int UserId { get; set; }
    public string EventType { get; set; } = default!;
}

