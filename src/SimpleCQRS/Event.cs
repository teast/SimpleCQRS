namespace Teast.SimpleCQRS;

/// <summary>Represents an event for an <see cref="Aggregate{TEvent, TData, TID}"/></summary>
public abstract record Event
{
    /// <summary>This events version</summary>
    public int Version { get; set; }
    /// <summary>When this event was created</summary>
    public virtual DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

