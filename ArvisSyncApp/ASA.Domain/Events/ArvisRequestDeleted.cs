using ASA.Domain.Interfaces;

namespace ASA.Domain.Events;

public sealed record ArvisRequestDeleted(
    Guid AggregateId,
    DateTimeOffset OccuredAt) : IDomainEvent;