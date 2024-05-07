namespace UserApi.CQRS.Commands;

public record ChangeNameCommand(int Id, string Name, int ExpectedVersion);

