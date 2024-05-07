namespace UserApi.CQRS.Commands;

public record ChangeEmailCommand(int Id, string Email, int ExpectedVersion);

