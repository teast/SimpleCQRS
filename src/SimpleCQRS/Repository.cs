namespace SimpleCQRS;

/// <summary>
/// <see cref="Repository{TAggregate, TStorage, TEvent, TData, TID}"/> is the nav between changes in <see cref="Repository{TAggregate, TStorage, TEvent, TData, TID}"/> and the data storage in <see cref="IStorage{TEvent, TData, TID}"/>
/// </summary>
public abstract class Repository<TAggregate, TStorage, TEvent, TData, TID>
    where TAggregate: Aggregate<TEvent, TData, TID>
    where TStorage : IStorage<TEvent, TData, TID>
    where TEvent : Event
    where TData : Data<TID>
{
    /// <summary>Define how to create an <typeparamref name="TData"/></summary>
    protected abstract Func<TID, TData> CreateData { get; }

    /// <summary>Define how to create an <typeparamref name="TAggregate"/></summary>
    protected abstract Func<TData, TAggregate> CreateAggregate { get; }

    /// <summary>Set to data storage to be used when fetching/storing events and snapshots</summary>
    protected abstract TStorage Storage { get; }

    /// <summary>
    /// Will fetch all new changes from <paramref name="aggregate"/> and store it using <see cref="Storage"/>
    /// </summary>
    public virtual async Task SaveAsync(TAggregate aggregate, int expectedVersion)
    {
        if (!aggregate.HasChanges)
            return;

        var currentVersion = (await Storage.GetMaxVersionAsync(aggregate.Id));
        if (currentVersion != expectedVersion)
            throw new ConcurrencyException($"{aggregate.Id}", expectedVersion, currentVersion);
        while (aggregate.HasChanges)
        {
            if (!aggregate.TryGetNextChange(out var @event))
                break;

            await Storage.AddEventAsync(aggregate.Id, @event!.Version, @event);
        }

        await Storage.UpdateSnapshotAsync(aggregate.Id, aggregate.LatestSnapshotVersion, aggregate.ToSnapshot());
        await Storage.SaveChangesAsync();
    }

    /// <summary>
    /// Load snapshot and events from storage and generate an instance of <typeparamref name="TAggregate"/>
    /// </summary>
    public virtual async Task<TAggregate> GetAsync(TID id)
    {
        var snapshot = (await Storage.GetSnapshotAsync(id)) ?? CreateData(id) with { Version = 0 };
        var aggregate = CreateAggregate(snapshot);
        var events = await Storage.GetEventsAsync(id, snapshot);
        aggregate.LoadFromHistory(events);
        return aggregate;
    }
}

