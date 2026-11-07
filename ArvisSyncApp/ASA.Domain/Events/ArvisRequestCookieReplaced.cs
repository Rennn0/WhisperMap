using ASA.Domain.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Domain.Events;

public sealed record ArvisRequestCookieReplaced(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<KeyValueProperty> Cookies) : IDomainEvent;