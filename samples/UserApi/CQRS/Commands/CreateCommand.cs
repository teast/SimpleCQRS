namespace UserApi.CQRS.Commands;

public record CreateCommand(int Id, string Name, string Email, int Age);

