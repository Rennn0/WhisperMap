namespace ASA.Domain.Interfaces;

public interface IRepository<TAggregate>
{
    Task<TAggregate?> GetAsync(object id, CancellationToken cancellationToken = default);
    Task<long> SaveAsync(TAggregate request, CancellationToken cancellationToken = default);
}