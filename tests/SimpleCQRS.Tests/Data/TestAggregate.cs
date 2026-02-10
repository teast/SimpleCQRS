namespace SimpleCQRS.Tests.Data;

public class TestAggregate(TestData data) : Aggregate<TestEventRecord, TestEvent, TestData, int>(data)
{
    public int NumberOfEventsApplied { get; set; }

    protected override void Apply(TestEventRecord record)
    {
        NumberOfEventsApplied++;
        base.Apply(record);
    }

    public void AddEvent()
    {
        AddEvent(new TestEventRecord { EventData = new TestEvent("Test event") });
    }

    public void AddRecord(TestEventRecord record)
        => AddEvent(record);

    public void AddEventWithData(TestEvent @event)
        => AddEvent(new TestEventRecord { EventData = @event });
}

