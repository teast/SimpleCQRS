using Teast.SimpleCQRS;
using UserApi.CQRS.Events;
using UserApi.Database.Models;

namespace UserApi.CQRS;

public class UserAggregate : Aggregate<Events.UserEvent, Database.Models.User, int>
{
    public UserAggregate(User data) : base(data)
    {
    }

    protected override void Apply(Events.UserEvent @event)
    {
        switch(@event)
        {
            case CreatedEvent e:
                Data.Name = e.Name;
                Data.Age = e.Age;
                Data.Email = e.Email;
                break;
            case AgeChangedEvent e:
                Data.Age = e.Age;
                break;
            case EmailChangedEvent e:
                Data.Email = e.Email;
                break;
            case NameChangedEvent e:
                Data.Name = e.Name;
                break;
            default:
                throw new NotImplementedException($"No handler implemented for event of type {@event.GetType()} in {nameof(Apply)}");
        }

        base.Apply(@event);
    }

    public void Create(int id, string name, string email, int age)
    {
        if (Version > 0)
            throw new InvalidStateException($"User with id {id} already exists!");

        AddEvent(new CreatedEvent(id, name, email, age));
    }

    public void ChangeName(string name)
    {
        if (Version == 0)
            throw new UserNotFoundException(Id, $"No user with id {Id} found in database");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty or only spaces");
        if (Data.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            return;

        AddEvent(new NameChangedEvent(name));
    }

    public void ChangeEmail(string email)
    {
        if (Version == 0)
            throw new UserNotFoundException(Id, $"No user with id {Id} found in database");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty or only spaces");
        if (Data.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
            return;

        AddEvent(new EmailChangedEvent(email));
    }

    public void ChangeAge(int age)
    {
        if (Version == 0)
            throw new UserNotFoundException(Id, $"No user with id {Id} found in database");
        if (age <= 0)
            throw new ArgumentException("Age cannot be zero or negative");
        if (age == Data.Age)
            return;

        AddEvent(new AgeChangedEvent(age));
    }
}

public class InvalidStateException(string Message) : Exception(Message);
#pragma warning disable CS9113
public class UserNotFoundException(int Id, string Message) : Exception(Message);
#pragma warning restore CS9113

