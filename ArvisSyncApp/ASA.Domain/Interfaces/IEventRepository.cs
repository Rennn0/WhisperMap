using ASA.Domain.Events;

namespace ASA.Domain.Interfaces;

public interface IEventRepository
{
    Task CreateAsync(ArvisRequestDomainEvent @event);
}