using ASA.Domain.Interfaces;

namespace ASA.Domain.Abstractions;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _uncommitedDomainEvents = [];

    public long Version { get; private set; }
    public IReadOnlyList<IDomainEvent> UncommitedDomainEvents => _uncommitedDomainEvents.AsReadOnly();

    protected void ApplyPersisted(IDomainEvent @event, long version)
    {
        Apply(@event);
        Version = version; 
    }

    public void MarkCommited()
    {
        Version += _uncommitedDomainEvents.Count;
        _uncommitedDomainEvents.Clear();
    }

    protected void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _uncommitedDomainEvents.Add(@event);
    }

    protected abstract void Apply(IDomainEvent @event);
}