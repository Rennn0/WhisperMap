using ASA.Domain.Interfaces;

namespace ASA.Domain.Abstractions;

public abstract class DomainEntity : Entity, IEventEmmiter<IEvent>
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? ModifiedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected bool IsNew { get; init; }
    private readonly List<IEvent> _events = [];

    public virtual void AddEvent(IEvent @event)
    {
        int i = _events.FindIndex(e => e.Action == @event.Action && e.Id == @event.Id);
        if (i < 0)
        {
            _events.Add(@event);
        }
        else
        {
            _events.RemoveAt(i);
            _events.Insert(i, @event);
        }
    }

    public void RemoveEvent(IEvent @event)
    {
        _events.Remove(@event);
    }

    public void ClearEvents()
    {
        _events.Clear();
    }

    public IReadOnlyCollection<IEvent> Events => _events.AsReadOnly();
}