namespace ASA.Domain.Interfaces;

public interface IEvent
{
    public Guid Id { get; }
    public string Action { get; }
}