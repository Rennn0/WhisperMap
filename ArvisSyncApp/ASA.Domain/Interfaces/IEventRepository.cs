using ASA.Domain.Abstractions;

namespace ASA.Domain.Interfaces;

public interface IEventRepository<TAggregate> : IRepository<TAggregate> where TAggregate : AggregateRoot;