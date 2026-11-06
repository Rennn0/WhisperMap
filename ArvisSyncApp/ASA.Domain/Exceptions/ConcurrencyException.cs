namespace ASA.Domain.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(Guid aggregateId,
        long expectedVersion,
        long? actualVersion,
        Exception? inner = null) : base(
        $"Aggregate {aggregateId} was expected at version {expectedVersion}, but is at {actualVersion?.ToString() ?? "a newer version"}.",
        inner)
    {
        AggregateId = aggregateId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public Guid AggregateId { get; }
    public long ExpectedVersion { get; }
    public long? ActualVersion { get; }
}