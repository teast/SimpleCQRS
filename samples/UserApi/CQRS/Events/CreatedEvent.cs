
namespace UserApi.CQRS.Events;

public record CreatedEvent(int Id, string Name, string Email, int Age) : UserEvent;

