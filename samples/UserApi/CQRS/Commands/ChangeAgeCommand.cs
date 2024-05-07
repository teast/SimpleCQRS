namespace UserApi.CQRS.Commands;

public record ChangeAgeCommand(int Id, int Age, int ExpectedVersion);

