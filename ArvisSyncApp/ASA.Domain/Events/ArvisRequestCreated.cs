using ASA.Domain.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Domain.Events;

public sealed record ArvisRequestCreated(
    Guid AggregateId,
    DateTimeOffset OccuredAt,
    string Title,
    IReadOnlyList<KeyValueProperty> Headers,
    IReadOnlyList<KeyValueProperty> Cookies) : IDomainEvent;