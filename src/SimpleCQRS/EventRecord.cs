namespace Teast.SimpleCQRS;

/// <summary>Represents an event record for an <see cref="Aggregate{TEvent, TEventData, TData, TID}"/></summary>
/// <remarks>Event record is the holder of actual event data. It could be represented in a table for example</remarks>
public abstract record EventRecord<TEventData>
{
    /// <summary>This events version</summary>
    public int Version { get; set; }
    /// <summary>When this event was created</summary>
    public virtual DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>The actual data for given event</summary>
    public virtual TEventData Event { get; set; } = default!;
}

