namespace Teast.SimpleCQRS;

/// <summary>Represents data for an <see cref="Teast.SimpleCQRS.Aggregate{TEvent, TData, TID}"/></summary>
/// <param name="Id">Id for given entity</param>
public abstract record Data<TID>(TID Id)
{
    /// <summary>Latest version for this entity</summary>
    public int Version { get; internal set; }
    /// <summary>Should contain version that the latest snapshot has</summary>
    public int LatestSnapshotVersion { get; set; }
    /// <summary>When last event was created</summary>
    public DateTimeOffset LastModified { get; internal set; }
}

