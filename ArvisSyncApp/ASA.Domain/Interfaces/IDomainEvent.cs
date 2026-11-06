namespace ASA.Domain.Interfaces;

public interface IDomainEvent
{
    Guid AggregateId { get; }
    DateTimeOffset OccuredAt { get; }
}