using ASA.Domain.Abstractions;
using ASA.Domain.Interfaces;

namespace ASA.Domain.Events;

public abstract class ArvisRequestDomainEvent : Entity, IEvent
{
    protected ArvisRequestDomainEvent(Guid id, Guid arvisRequestId, string action)
    {
        Id = id;
        ArvisRequestId = arvisRequestId;
        Action = action;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Action { get; }
    public Guid ArvisRequestId { get; }
    private DateTimeOffset CreatedAt { get; }
}