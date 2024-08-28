namespace SimpleCQRS.Tests.Data;

public class TestAggregate(TestData data) : Aggregate<TestEvent, TestData, int>(data)
{
    public int NumberOfEventsApplied { get; set; }

    protected override void Apply(TestEvent @event)
    {
        NumberOfEventsApplied++;
        base.Apply(@event);
    }

    public void AddEvent()
    {
        AddEvent(new TestEvent("Test event"));
    }
    public void AddEventWithData(TestEvent @event)
        => AddEvent(@event);
}

