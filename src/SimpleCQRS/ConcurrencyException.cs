namespace Teast.SimpleCQRS;

/// <summary>
/// This is thrown if an <see cref="Aggregate{TEvent, TData, TID}"/>'s expected version is not matching the current version from data storage
/// </summary>
public class ConcurrencyException : AggregateException
{
    /// <summary>Id of the <see cref="Aggregate{TEvent, TData, TID}" /></summary>
    public string Id { get; } = default!;
    /// <summary>The expected version when this exception occurred</summary>
    public int ExpectedVersion { get; }
    /// <summary>The version in data storage when this exception occurred</summary>
    public int CurrentVersion { get; }

    /// <inheritdoc />
    public ConcurrencyException() {}
    /// <inheritdoc />
    public ConcurrencyException(string message) : base(message) {}
    /// <inheritdoc />
    public ConcurrencyException(Exception innerException) : base(innerException) {}
    /// <inheritdoc />
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException) {}
    /// <summary>Initialize a new instance with aggregate values set</summary>
    public ConcurrencyException(string id, int expectedVersion, int currentVersion)
    {
        Id = id;
        ExpectedVersion = expectedVersion;
        CurrentVersion = currentVersion;
    }

    /// <summary>Initialize a new instance with aggregate values set and an inner exception set</summary>
    public ConcurrencyException(string id, int expectedVersion, int currentVersion, Exception innerException) : base(innerException)
    {
        Id = id;
        ExpectedVersion = expectedVersion;
        CurrentVersion = currentVersion;
    }
}

