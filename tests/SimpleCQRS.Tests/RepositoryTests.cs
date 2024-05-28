namespace SimpleCQRS.Tests;

public class RepositoryTests
{
#region SaveAsync
    [Test]
    public async Task SaveAsync_AggregateWithNoChanges_Then_SaveChanges_ShouldNotBeTriggered()
    {
        var storage = new Mock<ITestStorage>();
        var sut = new TestRepository(
                _ => throw new InvalidProgramException("Should not be called in test"),
                _ => throw new InvalidProgramException("Should not be called in test"),
                storage.Object);

        // Arrange
        var aggregate = new TestAggregate(new TestData(42));

        // Act
        await sut.SaveAsync(aggregate, aggregate.Version);

        // Assert
        storage.Verify(m => m.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void SaveAsync_Aggregate_With_MismatchVersion_Then_Exception()
    {
        var storage = new Mock<ITestStorage>();
        var sut = new TestRepository(
                _ => throw new InvalidProgramException("Should not be called in test"),
                _ => throw new InvalidProgramException("Should not be called in test"),
                storage.Object);

        // Arrange
        var aggregate = new TestAggregate(new TestData(42));
        aggregate.AddEvent();
        storage.Setup(m => m.GetMaxVersionAsync(aggregate.Id)).ReturnsAsync(999);

        // Act
        Assert.ThrowsAsync<ConcurrencyException>(async () => await sut.SaveAsync(aggregate, aggregate.Version));

        // Assert
        storage.Verify(m => m.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task SaveAsync_Aggregate_With_Changes_Then_SaveChanges()
    {
        var storage = new Mock<ITestStorage>();
        var sut = new TestRepository(
                _ => throw new InvalidProgramException("Should not be called in test"),
                _ => throw new InvalidProgramException("Should not be called in test"),
                storage.Object);

        // Arrange
        var aggregate = new TestAggregate(new TestData(42));
        aggregate.AddEvent();
        aggregate.AddEvent();
        aggregate.AddEvent();
        storage.Setup(m => m.GetMaxVersionAsync(aggregate.Id)).ReturnsAsync(0);

        // Act
        await sut.SaveAsync(aggregate, 0);

        // Assert
        storage.Verify(m => m.AddEventAsync(aggregate.Id, It.Is<TestEvent>(e => e.Version == 1)), Times.Once);
        storage.Verify(m => m.AddEventAsync(aggregate.Id, It.Is<TestEvent>(e => e.Version == 2)), Times.Once);
        storage.Verify(m => m.AddEventAsync(aggregate.Id, It.Is<TestEvent>(e => e.Version == 3)), Times.Once);
        storage.Verify(m => m.UpdateSnapshotAsync(aggregate.Id, aggregate.LatestSnapshotVersion, aggregate.ToSnapshot()), Times.Once);
        storage.Verify(m => m.SaveChangesAsync(), Times.Once);
    }
#endregion
#region GetAsync
    [Test]
    public async Task GetAsync_NonExistingEntity_Then_ReturnEntityWithVersionSetToZero()
    {
        const int id = 42;
        var createDataWasCalled = false;
        var createAggregateWasCalled = false;
        var storage = new Mock<ITestStorage>();
        var sut = new TestRepository(
                id =>
                {
                    createDataWasCalled = true;
                    return new TestData(id);
                },
                data =>
                {
                    createAggregateWasCalled = true;
                    return new TestAggregate(data);
                },
                storage.Object);

        // Arrange
        storage.Setup(m => m.GetSnapshotAsync(id)).ReturnsAsync((TestData?)null);
        storage.Setup(m => m.GetEventsAsync(id, It.IsAny<TestData>())).ReturnsAsync([]);

        // Act
        var result = await sut.GetAsync(id);

        // Assert
        createDataWasCalled.Should().BeTrue();
        createAggregateWasCalled.Should().BeTrue();
        result.Id.Should().Be(id);
        result.Version.Should().Be(0);
    }

    [Test]
    public async Task GetAsync_Existing_NoSnapshot_ReturnEntity()
    {
        const int id = 42;
        var createDataWasCalled = false;
        var createAggregateWasCalled = false;
        var storage = new Mock<ITestStorage>();
        var sut = new TestRepository(
                id =>
                {
                    createDataWasCalled = true;
                    return new TestData(id);
                },
                data =>
                {
                    createAggregateWasCalled = true;
                    return new TestAggregate(data);
                },
                storage.Object);
        var events = new List<TestEvent>();
        for (var i = 1; i < 4; i++)
        {
            var e = new TestEvent();
            SetProperty(e, nameof(e.Version), i);
            events.Add(e);
        }

        // Arrange
        storage.Setup(m => m.GetSnapshotAsync(id)).ReturnsAsync((TestData?)null);
        storage.Setup(m => m.GetEventsAsync(id, It.IsAny<TestData>())).ReturnsAsync(events);

        // Act
        var result = await sut.GetAsync(id);

        // Assert
        createDataWasCalled.Should().BeTrue();
        createAggregateWasCalled.Should().BeTrue();
        result.Id.Should().Be(id);
        result.Version.Should().Be(3);
    }

    [Test]
    public async Task GetAsync_Existing_WithSnapshot_ReturnEntity()
    {
        const int id = 42;
        const int MaxEventVersions = 4;

        var createAggregateWasCalled = false;
        var storage = new Mock<ITestStorage>();
        var sut = new TestRepository(
                _ => throw new InvalidProgramException("Should not be called in this test"),
                data =>
                {
                    createAggregateWasCalled = true;
                    return new TestAggregate(data);
                },
                storage.Object);
        var events = new List<TestEvent>();
        for (var i = 1; i < MaxEventVersions + 1; i++)
        {
            var e = new TestEvent();
            SetProperty(e, nameof(e.Version), i);
            events.Add(e);
        }

        var snapshot = new TestData(id) { LatestSnapshotVersion = MaxEventVersions - 1 };
        SetProperty(snapshot, nameof(snapshot.Version), MaxEventVersions-1);

        // Arrange
        storage.Setup(m => m.GetSnapshotAsync(id)).ReturnsAsync(snapshot);
        storage.Setup(m => m.GetEventsAsync(id, It.IsAny<TestData>())).ReturnsAsync(events);

        // Act
        var result = await sut.GetAsync(id);

        // Assert
        createAggregateWasCalled.Should().BeTrue();
        result.Id.Should().Be(id);
        result.Version.Should().Be(4);
        result.NumberOfEventsApplied.Should().Be(1, "Because Snapshot's version is one step below MaxEventVersion we should only have loaded the last event");
    }
#endregion

    private void SetProperty(object data, string propertyName, object value)
    {
        var prop = data.GetType().GetProperty(propertyName)!;
        prop.SetValue(data, value);
    }
}
