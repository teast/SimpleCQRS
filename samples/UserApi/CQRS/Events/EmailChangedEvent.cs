
namespace UserApi.CQRS.Events;

public record EmailChangedEvent(string Email) : UserEvent;

