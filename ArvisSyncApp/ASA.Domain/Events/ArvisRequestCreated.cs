namespace ASA.Domain.Events;

public class ArvisRequestCreated : ArvisRequestDomainEvent
{
    public ArvisRequest ArvisRequest { get; }

    public ArvisRequestCreated(ArvisRequest arvisRequest) : base(Guid.NewGuid(), arvisRequest.Id,
        nameof(ArvisRequestCreated)) => ArvisRequest = arvisRequest;
}