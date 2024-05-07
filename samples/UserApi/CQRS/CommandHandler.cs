using UserApi.CQRS.Commands;

namespace UserApi.CQRS;

public class CommandHandler(UserRepository repository)
{
    public async Task Handle(CreateCommand command)
    {
        var aggregate = await repository.GetAsync(command.Id);
        aggregate.Create(command.Id, command.Name, command.Email, command.Age);
        await repository.SaveAsync(aggregate, 0);
    }

    public async Task Handle(ChangeNameCommand command)
    {
        var aggregate = await repository.GetAsync(command.Id);
        aggregate.ChangeName(command.Name);
        await repository.SaveAsync(aggregate, command.ExpectedVersion);
    }

    public async Task Handle(ChangeEmailCommand command)
    {
        var aggregate = await repository.GetAsync(command.Id);
        aggregate.ChangeEmail(command.Email);
        await repository.SaveAsync(aggregate, command.ExpectedVersion);
    }

    public async Task Handle(ChangeAgeCommand command)
    {
        var aggregate = await repository.GetAsync(command.Id);
        aggregate.ChangeAge(command.Age);
        await repository.SaveAsync(aggregate, command.ExpectedVersion);
    }
}
