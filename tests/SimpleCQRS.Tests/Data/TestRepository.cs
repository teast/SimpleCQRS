
namespace SimpleCQRS.Tests.Data;

public class TestRepository : Repository<TestAggregate, TestEvent, TestData, int>
{
    protected override Func<int, TestData> CreateData { get; }

    protected override Func<TestData, TestAggregate> CreateAggregate { get; }

    protected override ITestStorage Storage { get; }

    public TestRepository(Func<int, TestData> createData, Func<TestData, TestAggregate> createAggregate, ITestStorage storage)
    {
        CreateData = createData;
        CreateAggregate = createAggregate;
        Storage = storage;
    }
}

