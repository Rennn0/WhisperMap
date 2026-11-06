using ASA.Domain.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Domain.Events;

public sealed record ArvisRequestCookieReplaced(
    Guid AggregateId,
    DateTimeOffset OccuredAt,
    IReadOnlyList<KeyValueProperty> Cookies) : IDomainEvent;