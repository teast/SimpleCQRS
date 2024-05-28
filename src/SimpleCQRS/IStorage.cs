namespace Teast.SimpleCQRS;

/// <summary>Represents an data storage for reading and storing events and snapshots</summary>
public interface IStorage<TEvent, TData, TID>
    where TEvent : Event
    where TData : Data<TID>
{
    /// <summary>Get latest version for given id from data storage</summary>
    Task<int> GetMaxVersionAsync(TID aggregateId);
    /// <summary>Adds given events to the data storage</summary>
    Task AddEventAsync(TID aggregateId, TEvent @event);
    /// <summary>
    /// Will be called after new events have been stored and before <see cref="SaveChangesAsync"/> is called.
    /// If you want to have snapshot (or projection) support you could add logic in here for storing them
    /// </summary>
    Task UpdateSnapshotAsync(TID aggregateId, int latestSnapshotVersion, TData data);
    /// <summary>events and snapshot have been stored, time to write the actual changes to data storage</summary>
    Task SaveChangesAsync();
    /// <summary>
    /// Will return latest snapshot for given aggregate id.
    /// If you do not want to use snapshots (or projection) you can just return null here
    /// </summary>
    Task<TData?> GetSnapshotAsync(TID aggregateId);
    /// <summary>
    /// Will load all events for given aggregate id.
    /// Only events with <see cref="Event.Version"/> higher than <see cref="Data{TID}.LatestSnapshotVersion"/> should be returned
    /// </summary>
    Task<IEnumerable<TEvent>> GetEventsAsync(TID aggregateId, TData snapshot);
}

