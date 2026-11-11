using ASA.Domain.Interfaces;

namespace ASA.Domain.Abstractions;

public abstract class AggregateRoot : EntityGuid
{
    private readonly List<IDomainEvent> _uncommitedDomainEvents = [];

    public long Version { get; private set; }
    public IReadOnlyList<IDomainEvent> UncommitedDomainEvents => _uncommitedDomainEvents.AsReadOnly();

    protected void ApplyPersisted(IDomainEvent @event, long version)
    {
        ApplyCore(@event);
        Version = version; 
    }

    public void MarkCommited()
    {
        Version += _uncommitedDomainEvents.Count;
        _uncommitedDomainEvents.Clear();
    }

    protected void Raise(IDomainEvent @event)
    {
        ApplyCore(@event);
        _uncommitedDomainEvents.Add(@event);
    }

    protected abstract void ApplyCore(IDomainEvent @event);
}