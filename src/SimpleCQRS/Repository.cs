namespace Teast.SimpleCQRS;

/// <summary>
/// <see cref="Repository{TAggregate, TEventRecord, TEventData, TData, TID}"/> is the nav between changes in <see cref="Repository{TAggregate, TEventRecord, TEventData, TData, TID}"/> and the data storage in <see cref="IStorage{TEventRecord, TEventData, TData, TID}"/>
/// </summary>
public abstract class Repository<TAggregate, TEventRecord, TEventData, TData, TID>
    where TAggregate: Aggregate<TEventRecord, TEventData, TData, TID>
    where TEventRecord : EventRecord<TEventData>
    where TData : Data<TID>
{
    /// <summary>Define how to create an <typeparamref name="TData"/></summary>
    protected abstract Func<TID, TData> CreateData { get; }

    /// <summary>Define how to create an <typeparamref name="TAggregate"/></summary>
    protected abstract Func<TData, TAggregate> CreateAggregate { get; }

    /// <summary>Set to data storage to be used when fetching/storing events and snapshots</summary>
    protected abstract IStorage<TEventRecord, TEventData, TData, TID> Storage { get; }

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

            await Storage.AddEventAsync(aggregate.Id, @event!);
        }

        await Storage.UpdateSnapshotAsync(aggregate.Id, aggregate.LatestSnapshotVersion, aggregate.ToSnapshot());
        await Storage.SaveChangesAsync();
    }

    /// <summary>
    /// Load events with timestamp before <paramref name="upToDate"/> from storage and generate an instance of <typeparamref name="TAggregate"/>
    /// </summary>
    /// <param name="id">Unique id for entity to load events for</param>
    /// <param name="upToDate">Load events up to given date</param>
    public virtual async Task<TAggregate> GetAsync(TID id, DateTimeOffset upToDate)
    {
        var aggregate = CreateAggregate(CreateData(id) with { Version = 0 });
        var events = await Storage.GetEventsBeforeAsync(id, upToDate);
        aggregate.LoadFromHistory(events);
        return aggregate;
    }

    /// <summary>
    /// Load snapshot and events from storage and generate an instance of <typeparamref name="TAggregate"/>
    /// </summary>
    /// <param name="id">Unique id for entity to load events for</param>
    /// <param name="skipSnapshot">set to true to load latest snapshot and then all events after that snapshot</param>
    public virtual async Task<TAggregate> GetAsync(TID id, bool skipSnapshot = false)
    {
        var snapshot = skipSnapshot switch
        {
            false => (await Storage.GetSnapshotAsync(id)) ?? CreateData(id) with { Version = 0 },
            true => CreateData(id) with { Version = 0 }
        };

        var aggregate = CreateAggregate(snapshot);
        var events = await Storage.GetEventsAsync(id, snapshot);
        aggregate.LoadFromHistory(events);
        return aggregate;
    }
}

