using ASA.Domain.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Domain.Events;

public sealed record ArvisRequestHeaderReplaced(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<KeyValueProperty> Headers) : IDomainEvent;