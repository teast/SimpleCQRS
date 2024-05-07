using SimpleCQRS;

namespace UserApi.CQRS.Events;

// All events related to user will inherit from this base event
public abstract record UserEvent : Event;
