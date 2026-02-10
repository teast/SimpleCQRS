namespace SimpleCQRS.Tests.Data;

public record TestEvent(string Name);
public record TestEventRecord() : EventRecord<TestEvent>;

