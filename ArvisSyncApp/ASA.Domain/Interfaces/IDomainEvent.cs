namespace ASA.Domain.Interfaces;

public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccurredAt { get; }
}