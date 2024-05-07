
namespace UserApi.CQRS.Events;

public record AgeChangedEvent(int Age) : UserEvent;

