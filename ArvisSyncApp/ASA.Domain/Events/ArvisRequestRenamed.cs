using ASA.Domain.Interfaces;

namespace ASA.Domain.Events;

public sealed record ArvisRequestRenamed(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string NewTitle) : IDomainEvent;