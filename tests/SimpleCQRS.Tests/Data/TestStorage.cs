namespace SimpleCQRS.Tests.Data;

public interface ITestStorage : IStorage<TestEventRecord, TestEvent, TestData, int>
{
}

