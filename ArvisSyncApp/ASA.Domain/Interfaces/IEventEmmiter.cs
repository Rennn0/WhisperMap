namespace ASA.Domain.Interfaces;

public interface IEventEmmiter<T> where T : IEvent
{
    public void AddEvent(T @event);
    public void RemoveEvent(T @event);
    public void ClearEvents();
    public IReadOnlyCollection<T> Events { get; }
}