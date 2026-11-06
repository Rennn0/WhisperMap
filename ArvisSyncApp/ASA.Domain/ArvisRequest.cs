using ASA.Domain.Abstractions;
using ASA.Domain.Events;
using ASA.Domain.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Domain;

public class ArvisRequest : DomainEntity, IAggregateRoot
{
    public KeyValueProperty? Headers { get; private set; }
    public KeyValueProperty? Cookies { get; private set; }
    public string? Title { get; private set; }

    private ArvisRequest(Guid id)
    {
        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    private ArvisRequest()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static ArvisRequest Create(Guid id)
    {
        ArvisRequest ar = new ArvisRequest(id) { IsNew = true };
        ar.AddEvent(new ArvisRequestCreated(ar));
        return ar;
    }

    public static ArvisRequest Create()
    {
        ArvisRequest ar = new ArvisRequest { IsNew = true };
        ar.AddEvent(new ArvisRequestCreated(ar));
        return ar;
    }
}