
using System.Reflection;

namespace SimpleCQRS.Tests;

public class AggregateTests
{
    private readonly Fixture _fixture = new ();

    [Test]
    public void Ctor_ShouldInitDataCorrectly()
    {
        // Arrange
        var expected = _fixture.Create<TestData>();

        // Act
        var aggregate = new TestAggregate(expected);

        // Assert
        var result = (TestData)aggregate.GetType().GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(aggregate)!;
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ToSnapshoit_ShouldReturnDataCorrectly()
    {
        // Arrange
        var expected = _fixture.Create<TestData>();

        // Act
        var aggregate = new TestAggregate(expected);

        // Assert
        aggregate.ToSnapshot().Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Id_ShouldReturnDataValue()
    {
        // Arrange
        var expected = _fixture.Create<TestData>();

        // Act
        var aggregate = new TestAggregate(expected);

        // Assert
        aggregate.Id.Should().Be(expected.Id);
    }

    [Test]
    public void Version_ShouldReturnDataValue()
    {
        // Arrange
        var expected = _fixture.Create<TestData>();

        // Act
        var aggregate = new TestAggregate(expected);

        // Assert
        aggregate.Version.Should().Be(expected.Version);
    }

    [Test]
    public void LatestSnapshotVersion_ShouldReturnDataValue()
    {
        // Arrange
        var expected = _fixture.Create<TestData>();

        // Act
        var aggregate = new TestAggregate(expected);

        // Assert
        aggregate.LatestSnapshotVersion.Should().Be(expected.LatestSnapshotVersion);
    }

    [Test]
    public void HasChanges_WithNoChanges_ShouldReturn_False()
    {
        // Arrange && Act
        var sut = new TestAggregate(_fixture.Create<TestData>());

        // Assert
        sut.HasChanges.Should().BeFalse();
    }

    [Test]
    public void HasChanges_WithChanges_ShouldReturn_True()
    {
        // Arrange && Act
        var sut = new TestAggregate(_fixture.Create<TestData>());
        sut.AddEvent();

        // Assert
        sut.HasChanges.Should().BeTrue();
    }

    [Test]
    public void ApplyEvent_NoPreviousEvents_Should_AddNewEvent_AndApplyIt()
    {
        // Arrange
        var sut = new TestAggregate(new TestData(42));

        // Act
        sut.AddEvent();

        // Assert
        sut.TryGetNextChange(out var result).Should().BeTrue();
        result!.Version.Should().Be(1);
        sut.NumberOfEventsApplied.Should().Be(1);
    }

    [Test]
    public void ApplyEvent_PreviousEvents_Should_AddNewEvent_AndApplyIt()
    {
        // Arrange
        var sut = new TestAggregate(new TestData(42));
        sut.AddEvent();
        sut.NumberOfEventsApplied = 0;

        // Act
        sut.AddEvent();

        // Assert
        sut.TryGetNextChange(out var result).Should().BeTrue();
        result!.Version.Should().Be(2);
        sut.NumberOfEventsApplied.Should().Be(1);
    }

    [Test]
    public void Apply_Should_UpdateVersionAndLastModified()
    {
        // Arrange
        var sut = new TestAggregate(new TestData(42));
        var expected = new TestEvent();

        // Act
        sut.AddEventWithData(expected);

        // Assert
        var result = (TestData)sut.GetType().GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(sut)!;
        result.Version.Should().Be(1);
        result.LastModified.Should().Be(expected.Timestamp);
    }

    [Test]
    public void LoadFromHistory_Should_OnlyLoadEventsAfterCurrentVersion()
    {
        var data = new TestData(42);
        SetProperty(data, nameof(data.Version), 3);

        var events = new List<TestEvent>();
        for(var i = 1; i <= 4; i++)
        {
            var e = new TestEvent();
            SetProperty(e, nameof(e.Version), i);
            events.Add(e);
        }

        // Arrange
        var sut = new TestAggregate(data);

        // Act
        var loadFromHistory = sut.GetType().GetMethod("LoadFromHistory", BindingFlags.Instance | BindingFlags.NonPublic)!;
        loadFromHistory.Invoke(sut, [ events ]);

        // Assert
        sut.HasChanges.Should().BeFalse();
        sut.NumberOfEventsApplied.Should().Be(1);
        sut.Version.Should().Be(4);
    }

    private void SetProperty(object data, string propertyName, object value)
    {
        var prop = data.GetType().GetProperty(propertyName)!;
        prop.SetValue(data, value);
    }
}
