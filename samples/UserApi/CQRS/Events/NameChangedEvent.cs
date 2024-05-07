
namespace UserApi.CQRS.Events;

public record NameChangedEvent(string Name) : UserEvent;

