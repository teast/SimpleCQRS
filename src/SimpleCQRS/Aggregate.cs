namespace Teast.SimpleCQRS;

/// <summary>
/// Aggregate contain the logic for modifying entity that the given aggregate represents
/// </summary>
public abstract class Aggregate<TEvent, TData, TID>
    where TEvent : Event
    where TData : Data<TID>
{
    private readonly Stack<TEvent> _changes = new ();

    /// <summary>Represents data that the given entity has</summary>
    protected TData Data { get; set; }

    /// <summary>Initialize a new instance of <see cref="Aggregate{TEvent, TData, TID}"/></summary>
    public Aggregate(TData data)
    {
        Data = data with {};
        OriginalVersion = Data.Version;
    }

    /// <summary>Will try and get next event that have not yet been stored</summary>
    public bool TryGetNextChange(out TEvent? result) => _changes.TryPop(out result);

    /// <summary>Get id for this <see cref="Aggregate{TEvent, TData, TID}"/></summary>
    public TID Id => Data.Id;

    /// <summary>Current version of this aggregate</summary>
    public int Version => Data.Version;

    /// <summary>Should contain version that the latest snapshot has</summary>
    public int LatestSnapshotVersion => Data.LatestSnapshotVersion;

    /// <summary>Version that the aggregate had when it was first read</summary>
    /// <remarks>If changes happens to the aggregate its <see cref="Version"/> will be updated. This will stay as it was when aggregate was first fetched from <see cref="Repository{TAggregate, TEvent, TData, TID}"/></remarks>
    public int OriginalVersion { get; }

    /// <summary>
    /// Return true if there are any events that have not yet been stored. Use <see cref="TryGetNextChange(out TEvent?)" /> to get this events
    /// </summary>
    public bool HasChanges => _changes.Any();

    /// <summary>Get snapshot data representing the current instance of this <see cref="Aggregate{TEvent, TData, TID}"/></summary>
    public virtual TData ToSnapshot()
        => Data with {};

    /// <summary>
    /// This method will be called when an event is applied to given aggregate.
    /// Override this method to apply changes to <see cref="Data"/>
    /// </summary>
    protected virtual void Apply(TEvent @event)
    {
        Data.Version = @event.Version;
        Data.LastModified = @event.Timestamp;
    }

    /// <summary>Add a new event to this aggregate</summary>
    protected virtual void AddEvent(TEvent @event)
        => ApplyEvent(@event with { Version = Version + 1 }, true);

    /// <summary>Load stored events that belongs to this aggregate</summary>
    internal void LoadFromHistory(IEnumerable<TEvent> history)
    {
        foreach (var e in history.Where(h => h.Version > Version).OrderBy(h => h.Version))
        {
            ApplyEvent(e, false);
        }
    }

    private void ApplyEvent(TEvent @event, bool isNew)
    {
         Apply(@event);
         if (isNew) _changes.Push(@event);
    }
}

