using SimpleCQRS;
using UserApi.Database.Models;

namespace UserApi.CQRS;

public class UserRepository(UserStorage storage) : Repository<UserAggregate, UserStorage, Events.UserEvent, Database.Models.User, int>
{
    protected override Func<int, User> CreateData => id => new User(id);

    protected override Func<User, UserAggregate> CreateAggregate => data => new UserAggregate(data);

    protected override UserStorage Storage => storage;
}
